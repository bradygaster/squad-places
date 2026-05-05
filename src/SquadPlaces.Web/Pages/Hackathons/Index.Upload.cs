using System.Text;
using SquadPlaces.Api.Endpoints;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;
using SquadPlaces.Web.Services;

namespace SquadPlaces.Web.Pages.Hackathons;

public partial class IndexModel
{
    [BindProperty]
    public RepositoryFolderUploadForm FolderUpload { get; set; } = new();

    [BindProperty]
    public List<IFormFile> FolderFiles { get; set; } = [];

    [BindProperty]
    public string? FolderManifestJson { get; set; }

    [BindProperty]
    public string? FolderUploadedPathsJson { get; set; }

    public async Task<IActionResult> OnPostUploadFolderAsync()
    {
        var relativePaths = BuildRelativePaths();
        var uploadedFiles = BuildUploadedFiles();

        if (relativePaths.Count == 0)
            ModelState.AddModelError(nameof(FolderFiles), "Select a git repo folder to upload.");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        // Phase 1: build two stream-handle lists — no bytes read yet.
        // IFormFile is backed by a disk-spooled temp file above the multipart threshold,
        // so OpenReadStream() is replayable: called once for analysis, once for storage copy.
        //
        // uploadedFileHandles — analysis subset: excludes bin/obj/node_modules/dist/coverage/.vs
        //   (noise for Squad SDK analysis)
        // allFileHandles      — storage copy set: excludes .git/bin/obj/dist/node_modules
        var uploadedFileHandles = uploadedFiles
            .Select(f => new HackathonUploadedFile { RelativePath = f.RelativePath, File = f.File })
            .ToList();

        var allFileHandles = BuildAllUploadedFiles()
            .Select(f => new HackathonUploadedFile { RelativePath = f.RelativePath, File = f.File })
            .ToList();

        var squadRepo = await _hackathonSquadService.AnalyzeRepositoryAsync(
            new HackathonRepositoryAnalysisRequest
            {
                Name = TrimOrEmpty(FolderUpload.Name),
                RepositoryUrl = TrimOrEmpty(FolderUpload.RepositoryUrl),
                Description = TrimOrEmpty(FolderUpload.Description),
                Tags = TrimOrEmpty(FolderUpload.Tags),
                DefaultBranch = TrimOrEmpty(FolderUpload.DefaultBranch),
                CommitSha = TrimOrEmpty(FolderUpload.CommitSha),
                HasSquadState = FolderUpload.HasSquadState,
                ToolSuggestions = TrimOrEmpty(FolderUpload.ToolSuggestions),
                McpSuggestions = TrimOrEmpty(FolderUpload.McpSuggestions),
                PluginSuggestions = TrimOrEmpty(FolderUpload.PluginSuggestions),
                AnalysisNotes = TrimOrEmpty(FolderUpload.AnalysisNotes),
                RelativePaths = relativePaths,
                Files = uploadedFileHandles
            },
            HttpContext.RequestAborted);

        var repo = squadRepo ?? await AnalyzeUploadedFolderAsync(relativePaths, uploadedFiles, FolderUpload);

        // Phase 2: analysis complete — register the repository metadata.
        await _storage.SaveHackathonRepositoryAsync(repo);

        // Phase 3: stream repo files into permanent storage (excluding .git only — bin/obj/node_modules/.vs preserved for storage fidelity).
        _logger.LogInformation("Copying files to storage. FolderFiles.Count={TotalUploaded}, allFileHandles.Count={Filtered}, uploadedFileHandles.Count={AnalysisFiltered}", 
            FolderFiles.Count, allFileHandles.Count, uploadedFileHandles.Count);
        await CopyFilesToRepoStorageAsync(repo.Id, allFileHandles, HttpContext.RequestAborted);

        StatusMessage = $"Analyzed and registered {repo.Name} ({allFileHandles.Count} file(s) stored out of {FolderFiles.Count} uploaded).";
        FolderFiles = [];
        FolderUpload = new RepositoryFolderUploadForm();
        await LoadAsync();
        return Page();
    }

    /// <summary>
    /// Streams file contents into permanent repo file storage.
    /// Called only after analysis succeeds, so storage is never polluted by
    /// failed or aborted analyses.
    /// Textual files are stored as UTF-8 strings; binary files are base-64 encoded
    /// using an ArrayPool-rented buffer to avoid allocating large byte arrays.
    /// </summary>
    private async Task CopyFilesToRepoStorageAsync(
        Guid repoId,
        IReadOnlyList<HackathonUploadedFile> files,
        CancellationToken cancellationToken)
    {
        const int bufferSize = 3 * 27306; // multiple of 3 for clean base-64 chunks (~80 KB)
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file.RelativePath))
                    continue;

                // Validate path — no escaping the repo root.
                var pathError = ApiValidation.ValidateRepoPath(file.RelativePath);
                if (pathError is not null)
                {
                    _logger.LogWarning("Skipping file with invalid path {Path}: {Error}", file.RelativePath, pathError);
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();

                string content;
                await using var stream = file.OpenReadStream();
                if (LooksTextual(file.RelativePath))
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: true, leaveOpen: false);
                    content = await reader.ReadToEndAsync(cancellationToken);
                }
                else
                {
                    // Encode binary file to base-64 in chunks using a rented buffer.
                    var sb = new StringBuilder();
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken)) > 0)
                        sb.Append(Convert.ToBase64String(buffer, 0, bytesRead));
                    content = sb.ToString();
                }

                try
                {
                    await _storage.UpsertRepoFileAsync(repoId, file.RelativePath, content);
                }
                catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Could not store file {Path} for repository {RepoId}.", file.RelativePath, repoId);
                }
            }
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task<HackathonRepository> AnalyzeUploadedFolderAsync(List<string> relativePaths, List<UploadedFile> files, RepositoryFolderUploadForm form)
    {
        var uploaded = files
            .Where(x => !string.IsNullOrWhiteSpace(x.RelativePath))
            .Where(x => !IsIgnoredUploadPath(x.RelativePath))
            .OrderBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rootFolder = relativePaths
            .Select(path => path.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

        var repoName = TrimOrEmpty(form.Name) ?? rootFolder ?? "Uploaded repository";
        var repoUrl = TrimOrEmpty(form.RepositoryUrl) ?? $"uploaded://{NormalizeSlug(rootFolder ?? repoName)}";

        var textFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in uploaded)
        {
            if (LooksTextual(item.RelativePath) && item.File.Length <= 256 * 1024)
            {
                var text = await ReadTextAsync(item.File);
                if (!string.IsNullOrWhiteSpace(text))
                    textFiles[item.RelativePath] = text;
            }
        }

        string? GetText(params string[] keys)
        {
            foreach (var key in keys)
            {
                if (textFiles.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
            return null;
        }

        var copilot = GetText(".github/copilot-instructions.md", ".copilot-instructions.md", "copilot-instructions.md");
        var squadTeam = GetText(".squad/team.md", "team.md");
        var decisions = GetText(".squad/decisions.md", "decisions.md");
        var logs = relativePaths.Where(p => p.StartsWith(".squad/log/", StringComparison.OrdinalIgnoreCase)).ToList();
        var orchestrationLogs = relativePaths.Where(p => p.StartsWith(".squad/orchestration-log/", StringComparison.OrdinalIgnoreCase)).ToList();

        var agentNames = relativePaths
            .Where(p => p.StartsWith(".github/agents/", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Split('/', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 3)
            .Select(parts => parts[2])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        var skillNames = relativePaths
            .Where(p => p.StartsWith("skills/", StringComparison.OrdinalIgnoreCase) || p.Contains("/skills/", StringComparison.OrdinalIgnoreCase))
            .Where(p => p.EndsWith("SKILL.md", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Split('/', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 2)
            .Select(parts => parts[^2])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        var docNames = relativePaths
            .Where(p => p.StartsWith("docs/", StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetFileName(p), "README.md", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        var toolSuggestions = BuildSuggestions(relativePaths, textFiles, ["playwright", "azd", "azure", "docker", "mcp", "vite", "esbuild", "storybook"]);
        var mcpSuggestions = BuildSuggestions(relativePaths, textFiles, ["mcp", "model context protocol", "azure ai", "github"]);
        var pluginSuggestions = BuildSuggestions(relativePaths, textFiles, ["skill", "plugin", "copilot", "extension"]);

        return new HackathonRepository
        {
            Id = Guid.NewGuid(),
            Name = repoName,
            RepositoryUrl = repoUrl,
            Description = TrimOrEmpty(form.Description),
            Tags = TrimOrEmpty(form.Tags),
            DefaultBranch = TrimOrEmpty(form.DefaultBranch),
            CommitSha = TrimOrEmpty(form.CommitSha),
            HasSquadState = form.HasSquadState || relativePaths.Any(p => p.StartsWith(".squad/", StringComparison.OrdinalIgnoreCase)),
            CopilotInstructionsSummary = Combine(copilot, SummaryOfPaths(relativePaths, ".github/copilot-instructions.md", ".copilot-instructions.md", "copilot-instructions.md")),
            AgentsSummary = Combine(agentNames.Count > 0 ? $"Agents discovered: {string.Join(", ", agentNames)}" : null, SummaryOfPaths(relativePaths, ".github/agents/")),
            SkillsSummary = Combine(skillNames.Count > 0 ? $"Skills discovered: {string.Join(", ", skillNames)}" : null, SummaryOfPaths(relativePaths, "skills/")),
            DocsSummary = Combine(docNames.Count > 0 ? $"Docs discovered: {string.Join(", ", docNames)}" : null, SummaryOfPaths(relativePaths, "docs/", "README.md")),
            ExistingSquadSummary = BuildExistingSquadSummary(squadTeam, decisions, logs, orchestrationLogs),
            SessionSummary = BuildSessionSummary(logs, orchestrationLogs),
            DirectiveSummary = Combine(TrimOrEmpty(form.AnalysisNotes), FirstNonEmptyLine(copilot, decisions, squadTeam)),
            ToolSuggestions = Combine(TrimOrEmpty(form.ToolSuggestions), toolSuggestions),
            McpSuggestions = Combine(TrimOrEmpty(form.McpSuggestions), mcpSuggestions),
            PluginSuggestions = Combine(TrimOrEmpty(form.PluginSuggestions), pluginSuggestions),
            AnalysisNotes = Combine(TrimOrEmpty(form.AnalysisNotes), BuildAnalysisNotes(relativePaths, textFiles)),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string? BuildAnalysisNotes(List<string> relativePaths, Dictionary<string, string> textFiles)
    {
        var notes = new List<string>();
        if (relativePaths.Any(p => p.StartsWith(".squad/", StringComparison.OrdinalIgnoreCase)))
            notes.Add("Existing squad metadata found.");
        if (relativePaths.Any(p => p.StartsWith(".github/", StringComparison.OrdinalIgnoreCase)))
            notes.Add("GitHub repo instructions present.");
        if (textFiles.ContainsKey(".github/copilot-instructions.md") || textFiles.ContainsKey(".copilot-instructions.md"))
            notes.Add("Copilot instructions captured.");
        if (notes.Count == 0)
            notes.Add("Repository folder uploaded successfully.");
        return string.Join(" ", notes);
    }

    private static string? BuildExistingSquadSummary(string? team, string? decisions, List<string> logs, List<string> orchestrationLogs)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(team)) parts.Add("team.md present");
        if (!string.IsNullOrWhiteSpace(decisions)) parts.Add("decisions.md present");
        if (logs.Count > 0) parts.Add($"{logs.Count} session log(s)");
        if (orchestrationLogs.Count > 0) parts.Add($"{orchestrationLogs.Count} orchestration log(s)");
        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private static string? BuildSessionSummary(List<string> logs, List<string> orchestrationLogs)
    {
        var parts = new List<string>();
        if (logs.Count > 0) parts.Add($"{logs.Count} session log(s)");
        if (orchestrationLogs.Count > 0) parts.Add($"{orchestrationLogs.Count} orchestration log(s)");
        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private static string? SummaryOfPaths(IEnumerable<string> relativePaths, params string[] prefixes)
    {
        var matches = relativePaths
            .Where(path => prefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .Take(8)
            .ToList();
        return matches.Count == 0 ? null : $"Files: {string.Join(", ", matches)}";
    }

    private static string? FirstNonEmptyLine(params string?[] values)
    {
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            var line = value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
            if (!string.IsNullOrWhiteSpace(line)) return line;
        }
        return null;
    }

    private static string? BuildSuggestions(IEnumerable<string> relativePaths, Dictionary<string, string> textFiles, string[] keywords)
    {
        var haystack = string.Join('\n', relativePaths).ToLowerInvariant() + "\n" + string.Join('\n', textFiles.Values).ToLowerInvariant();
        var hits = keywords.Where(keyword => haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToList();
        return hits.Count == 0 ? null : string.Join(", ", hits);
    }

    private static async Task<string> ReadTextAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        return await reader.ReadToEndAsync();
    }

    private static bool LooksTextual(string relativePath)
    {
        var ext = Path.GetExtension(relativePath).ToLowerInvariant();
        return ext is ".md" or ".txt" or ".json" or ".yaml" or ".yml" or ".cs" or ".csproj" or ".props" or ".targets" or ".xml" or ".js" or ".ts" or ".ps1" or ".sh" or ".html" or ".css" or ".razor";
    }

    private static string NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        while (normalized.Contains("//", StringComparison.Ordinal))
            normalized = normalized.Replace("//", "/", StringComparison.Ordinal);
        return normalized;
    }

    private static string NormalizeSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(slug.Length);
        var previousDash = false;
        foreach (var ch in slug)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                previousDash = false;
            }
            else if (!previousDash)
            {
                builder.Append('-');
                previousDash = true;
            }
        }
        return builder.ToString().Trim('-');
    }

    private static string? Combine(params string?[] parts)
    {
        var values = parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return values.Count == 0 ? null : string.Join("; ", values);
    }

    private List<string> BuildRelativePaths()
    {
        var manifest = DeserializePathsJson(FolderManifestJson);
        if (manifest.Count == 0)
            manifest = DeserializePathsJson(FolderUploadedPathsJson);

        var source = manifest.Count > 0
            ? manifest
            : FolderFiles.Select(file => file.FileName).ToList();

        return source
            .Select(NormalizeRelativePath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Where(path => !IsIgnoredUploadPath(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private List<UploadedFile> BuildUploadedFiles()
    {
        var uploadedPaths = DeserializePathsJson(FolderUploadedPathsJson);

        return FolderFiles
            .Select((file, index) => new UploadedFile(
                NormalizeRelativePath(uploadedPaths.Count > 0 && index < uploadedPaths.Count ? uploadedPaths[index] : file.FileName),
                file))
            .Where(x => !string.IsNullOrWhiteSpace(x.RelativePath))
            .Where(x => !IsIgnoredUploadPath(x.RelativePath))
            .ToList();
    }

    // Returns every uploaded file, excluding only the .git folder at the repo root.
    // Used for the storage copy pass — bin/obj/dist etc. are intentionally preserved
    // so the stored copy is a faithful replica of the uploaded repo.
    private List<UploadedFile> BuildAllUploadedFiles()
    {
        var uploadedPaths = DeserializePathsJson(FolderUploadedPathsJson);
        
        var allFiles = FolderFiles
            .Select((file, index) => new UploadedFile(
                NormalizeRelativePath(uploadedPaths.Count > 0 && index < uploadedPaths.Count ? uploadedPaths[index] : file.FileName),
                file))
            .Where(x => !string.IsNullOrWhiteSpace(x.RelativePath))
            .ToList();

        _logger.LogInformation("BuildAllUploadedFiles: {TotalFiles} files before filtering, {PathsAvailable} paths in uploadedPaths", 
            allFiles.Count, uploadedPaths.Count);

        var filtered = allFiles.Where(x => !IsGitPath(x.RelativePath)).ToList();
        
        var excluded = allFiles.Count - filtered.Count;
        if (excluded > 0)
        {
            _logger.LogInformation("BuildAllUploadedFiles: Excluded {ExcludedCount} files (.git only — bin/obj/node_modules/.vs preserved for storage fidelity)", excluded);
        }

        return filtered;
    }

    private static List<string> DeserializePathsJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static readonly HashSet<string> _ignoredFolderNames =
        new(StringComparer.OrdinalIgnoreCase) { ".git", "bin", "obj", "node_modules", ".vs" };

    private static bool IsIgnoredUploadPath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(segment => _ignoredFolderNames.Contains(segment));
    }

    // Matches paths that should never be stored: .git, bin, obj, dist, node_modules.
    private static bool IsGitPath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        return normalized.Equals(".git", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(".git/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TrimOrEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record UploadedFile(string RelativePath, IFormFile File);
}

public class RepositoryFolderUploadForm
{
    public string? Name { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public string? DefaultBranch { get; set; }
    public string? CommitSha { get; set; }
    public bool HasSquadState { get; set; }
    public string? CopilotInstructionsSummary { get; set; }
    public string? AgentsSummary { get; set; }
    public string? SkillsSummary { get; set; }
    public string? DocsSummary { get; set; }
    public string? ExistingSquadSummary { get; set; }
    public string? SessionSummary { get; set; }
    public string? DirectiveSummary { get; set; }
    public string? ToolSuggestions { get; set; }
    public string? McpSuggestions { get; set; }
    public string? PluginSuggestions { get; set; }
    public string? AnalysisNotes { get; set; }
}

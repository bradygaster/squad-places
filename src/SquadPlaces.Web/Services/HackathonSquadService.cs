using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Services;

public sealed class HackathonSquadService(ILogger<HackathonSquadService> logger, IWebHostEnvironment environment) : IHackathonSquadService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ILogger<HackathonSquadService> _logger = logger;
    private readonly string _nodeProjectDirectory = Path.Combine(environment.ContentRootPath, "Node");
    private readonly string _workerScriptPath = Path.Combine(environment.ContentRootPath, "Node", "hackathon-squad-worker.mjs");
    private readonly string _squadConfigPath = Path.Combine(environment.ContentRootPath, "Node", "squad.config.js");
    private readonly string _packageJsonPath = Path.Combine(environment.ContentRootPath, "Node", "package.json");

    public async Task<HackathonRepository?> AnalyzeRepositoryAsync(HackathonRepositoryAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasWorkerFiles())
            return null;

        var tempRoot = CreateTempRoot();

        try
        {
            var repositoryDirectory = Path.Combine(tempRoot, "repository");
            Directory.CreateDirectory(repositoryDirectory);
            await WriteUploadedFilesAsync(repositoryDirectory, request.Files, cancellationToken);

            var response = await RunWorkerAsync<HackathonRepositoryWorkerResult>(
                mode: "analyze",
                payload: new AnalyzeWorkerPayload
                {
                    RepositoryDirectory = repositoryDirectory,
                    Request = request
                },
                tempRoot,
                cancellationToken);

            if (response?.Result is null)
                return null;

            var result = response.Result;
            var repoName = FirstNonEmpty(result.Name, request.Name) ?? "Uploaded repository";
            var repoUrl = FirstNonEmpty(result.RepositoryUrl, request.RepositoryUrl) ?? $"uploaded://{NormalizeSlug(repoName)}";

            return new HackathonRepository
            {
                Id = Guid.NewGuid(),
                Name = repoName,
                RepositoryUrl = repoUrl,
                Description = FirstNonEmpty(result.Description, request.Description),
                Tags = FirstNonEmpty(result.Tags, request.Tags),
                DefaultBranch = FirstNonEmpty(result.DefaultBranch, request.DefaultBranch),
                CommitSha = FirstNonEmpty(result.CommitSha, request.CommitSha),
                HasSquadState = result.HasSquadState ?? request.HasSquadState || request.RelativePaths.Any(path => path.StartsWith(".squad/", StringComparison.OrdinalIgnoreCase)),
                CopilotInstructionsSummary = FirstNonEmpty(result.CopilotInstructionsSummary),
                AgentsSummary = FirstNonEmpty(result.AgentsSummary),
                SkillsSummary = FirstNonEmpty(result.SkillsSummary),
                DocsSummary = FirstNonEmpty(result.DocsSummary),
                ExistingSquadSummary = FirstNonEmpty(result.ExistingSquadSummary),
                SessionSummary = FirstNonEmpty(result.SessionSummary),
                DirectiveSummary = FirstNonEmpty(result.DirectiveSummary, request.AnalysisNotes),
                ToolSuggestions = FirstNonEmpty(result.ToolSuggestions, request.ToolSuggestions),
                McpSuggestions = FirstNonEmpty(result.McpSuggestions, request.McpSuggestions),
                PluginSuggestions = FirstNonEmpty(result.PluginSuggestions, request.PluginSuggestions),
                AnalysisNotes = FirstNonEmpty(result.AnalysisNotes, request.AnalysisNotes),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Squad SDK analysis failed after {TimeoutSeconds}s. Falling back to the built-in repository analyzer. Error: {Message}", 
                ex.Message?.Contains("Timeout") == true ? "600" : "unknown", 
                ex.Message);
            return null;
        }
        {
            DeleteDirectoryNoThrow(tempRoot);
        }
    }

    public async Task<HackathonBriefDraftResult?> GenerateBriefDraftAsync(HackathonBriefDraftRequest request, CancellationToken cancellationToken = default)
    {
        if (!HasWorkerFiles())
            return null;

        var tempRoot = CreateTempRoot();

        try
        {
            var response = await RunWorkerAsync<HackathonBriefDraftResult>(
                mode: "brief",
                payload: new BriefWorkerPayload
                {
                    Request = request
                },
                tempRoot,
                cancellationToken);

            return response?.Result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Squad SDK brief generation failed. Falling back to the built-in brief generator.");
            return null;
        }
        finally
        {
            DeleteDirectoryNoThrow(tempRoot);
        }
    }

    private bool HasWorkerFiles()
    {
        if (File.Exists(_workerScriptPath) && File.Exists(_packageJsonPath) && File.Exists(_squadConfigPath))
            return true;

        _logger.LogDebug("Hackathon Squad worker files are missing. Expected: {Worker}, {Package}, {Config}", _workerScriptPath, _packageJsonPath, _squadConfigPath);
        return false;
    }

    private async Task<WorkerEnvelope<TResult>?> RunWorkerAsync<TResult>(string mode, object payload, string tempRoot, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(tempRoot);

        var inputPath = Path.Combine(tempRoot, "input.json");
        var outputPath = Path.Combine(tempRoot, "output.json");
        await File.WriteAllTextAsync(inputPath, JsonSerializer.Serialize(payload, JsonOptions), cancellationToken);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                WorkingDirectory = _nodeProjectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.StartInfo.ArgumentList.Add(_workerScriptPath);
        process.StartInfo.ArgumentList.Add(mode);
        process.StartInfo.ArgumentList.Add(inputPath);
        process.StartInfo.ArgumentList.Add(outputPath);
        process.StartInfo.Environment["NODE_NO_WARNINGS"] = "1";
        process.StartInfo.Environment["NODE_OPTIONS"] = "--no-warnings";

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        WorkerEnvelope<TResult>? envelope = null;
        if (File.Exists(outputPath))
        {
            var outputJson = await File.ReadAllTextAsync(outputPath, cancellationToken);
            envelope = JsonSerializer.Deserialize<WorkerEnvelope<TResult>>(outputJson, JsonOptions);
        }

        if (process.ExitCode != 0)
        {
            _logger.LogWarning(
                "Hackathon Squad worker exited with code {ExitCode}. Error: {Error}. Diagnostics: {Diagnostics}. StdOut: {StdOut}. StdErr: {StdErr}",
                process.ExitCode,
                envelope?.Error,
                envelope?.Diagnostics,
                standardOutput,
                standardError);
            return null;
        }

        if (envelope is null)
        {
            _logger.LogWarning("Hackathon Squad worker succeeded but did not produce a readable output file. StdOut: {StdOut}. StdErr: {StdErr}", standardOutput, standardError);
            return null;
        }

        if (!envelope.Success)
        {
            _logger.LogWarning("Hackathon Squad worker reported failure: {Error}. Diagnostics: {Diagnostics}", envelope.Error, envelope.Diagnostics);
            return null;
        }

        return envelope;
    }

    private static async Task WriteUploadedFilesAsync(string repositoryDirectory, IEnumerable<HackathonUploadedFile> files, CancellationToken cancellationToken)
    {
        var normalizedRoot = Path.GetFullPath(repositoryDirectory);
        if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar))
            normalizedRoot += Path.DirectorySeparatorChar;

        foreach (var file in files)
        {
            var relativePath = NormalizeRelativePath(file.RelativePath);
            if (string.IsNullOrWhiteSpace(relativePath))
                continue;

            var fullPath = Path.GetFullPath(Path.Combine(repositoryDirectory, relativePath));
            if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Refusing to write uploaded content outside the temporary repository root: {relativePath}");

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            await using var src = file.OpenReadStream();
            await using var dst = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await src.CopyToAsync(dst, cancellationToken);
        }
    }

    private static string NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        while (normalized.Contains("//", StringComparison.Ordinal))
            normalized = normalized.Replace("//", "/", StringComparison.Ordinal);

        return normalized;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private static string NormalizeSlug(string value)
    {
        var buffer = new List<char>(value.Length);
        var previousDash = false;

        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer.Add(ch);
                previousDash = false;
            }
            else if (!previousDash)
            {
                buffer.Add('-');
                previousDash = true;
            }
        }

        return new string(buffer.ToArray()).Trim('-');
    }

    private static string CreateTempRoot() => Path.Combine(Path.GetTempPath(), "squadplaces", "hackathon-squad", Guid.NewGuid().ToString("n"));

    private static void DeleteDirectoryNoThrow(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Ignore teardown failures.
        }
    }

    private sealed class AnalyzeWorkerPayload
    {
        public required string RepositoryDirectory { get; init; }
        public required HackathonRepositoryAnalysisRequest Request { get; init; }
    }

    private sealed class BriefWorkerPayload
    {
        public required HackathonBriefDraftRequest Request { get; init; }
    }

    private sealed class WorkerEnvelope<T>
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public string? Diagnostics { get; init; }
        public T? Result { get; init; }
    }

    private sealed class HackathonRepositoryWorkerResult
    {
        public string? Name { get; init; }
        public string? RepositoryUrl { get; init; }
        public string? Description { get; init; }
        public string? Tags { get; init; }
        public string? DefaultBranch { get; init; }
        public string? CommitSha { get; init; }
        public bool? HasSquadState { get; init; }
        public string? CopilotInstructionsSummary { get; init; }
        public string? AgentsSummary { get; init; }
        public string? SkillsSummary { get; init; }
        public string? DocsSummary { get; init; }
        public string? ExistingSquadSummary { get; init; }
        public string? SessionSummary { get; init; }
        public string? DirectiveSummary { get; init; }
        public string? ToolSuggestions { get; init; }
        public string? McpSuggestions { get; init; }
        public string? PluginSuggestions { get; init; }
        public string? AnalysisNotes { get; init; }
    }
}

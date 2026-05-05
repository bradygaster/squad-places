using Microsoft.AspNetCore.Http;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Services;

public interface IHackathonSquadService
{
    /// <summary>
    /// Analyzes an uploaded repository and extracts Squad metadata, copilot instructions,
    /// agents, skills, documentation, and tool suggestions. The output seeds the hackathon
    /// repository record with signals that help brief authors craft precise Directives and
    /// scope constraints — and that help assigned teams understand the repository's existing
    /// state before they start evolving it. Returns null when the Node worker is unavailable;
    /// callers should fall back to the built-in static analyzer in that case.
    /// </summary>
    Task<HackathonRepository?> AnalyzeRepositoryAsync(HackathonRepositoryAnalysisRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a draft hackathon brief from a set of registered repositories and an
    /// optional seed. The draft populates Directive, suggested tools, skills, plugins,
    /// MCP servers, winner criteria, expected deliverables, and presentation instructions —
    /// giving brief authors a starting point to refine into a tight, unambiguous assignment.
    /// Returns null when the Node worker is unavailable.
    /// </summary>
    Task<HackathonBriefDraftResult?> GenerateBriefDraftAsync(HackathonBriefDraftRequest request, CancellationToken cancellationToken = default);
}

public sealed class HackathonRepositoryAnalysisRequest
{
    public string? Name { get; init; }
    public string? RepositoryUrl { get; init; }
    public string? Description { get; init; }
    public string? Tags { get; init; }
    public string? DefaultBranch { get; init; }
    public string? CommitSha { get; init; }
    public bool HasSquadState { get; init; }
    public string? ToolSuggestions { get; init; }
    public string? McpSuggestions { get; init; }
    public string? PluginSuggestions { get; init; }
    public string? AnalysisNotes { get; init; }
    public IReadOnlyList<string> RelativePaths { get; init; } = [];
    /// <summary>
    /// Stream handles for the uploaded files. Each entry holds only the relative
    /// path and an <see cref="IFormFile"/> whose stream is opened on demand —
    /// no bytes are materialised into memory.
    /// </summary>
    public IReadOnlyList<HackathonUploadedFile> Files { get; init; } = [];
}

/// <summary>
/// A single uploaded file represented as a stream handle.
/// The underlying <see cref="IFormFile"/> stream is opened lazily when needed;
/// it may be opened more than once (analysis pass, then storage-copy pass) because
/// ASP.NET Core buffers multipart form files to disk when they exceed the in-memory
/// threshold — rewinding is always safe.
/// </summary>
public sealed class HackathonUploadedFile
{
    public required string RelativePath { get; init; }
    public required IFormFile File { get; init; }

    /// <summary>Opens a fresh read stream from the beginning of the file.</summary>
    public Stream OpenReadStream()
    {
        var stream = File.OpenReadStream();
        if (stream.CanSeek)
            stream.Position = 0;
        return stream;
    }
}

public sealed class HackathonBriefDraftRequest
{
    public required IReadOnlyList<HackathonRepository> Repositories { get; init; }
    public HackathonBriefDraftSeed? Seed { get; init; }
}

public sealed class HackathonBriefDraftSeed
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Directive { get; init; }
    public string? LeadName { get; init; }
    public string? Roles { get; init; }
    public string? SuggestedTools { get; init; }
    public string? SuggestedMcpServers { get; init; }
    public string? SuggestedSkills { get; init; }
    public string? SuggestedPlugins { get; init; }
    public string? PresentationInstructions { get; init; }
    public string? WinnerCriteria { get; init; }
    public string? ExpectedDeliverables { get; init; }
    public string? OutOfScope { get; init; }
    public string? CheckInSchedule { get; init; }
}

public sealed class HackathonBriefDraftResult
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Directive { get; init; }
    public string? LeadName { get; init; }
    public string? Roles { get; init; }
    public string? SuggestedTools { get; init; }
    public string? SuggestedMcpServers { get; init; }
    public string? SuggestedSkills { get; init; }
    public string? SuggestedPlugins { get; init; }
    public string? PresentationInstructions { get; init; }
    public string? WinnerCriteria { get; init; }
    public string? ExpectedDeliverables { get; init; }
    public string? OutOfScope { get; init; }
    public string? CheckInSchedule { get; init; }
}

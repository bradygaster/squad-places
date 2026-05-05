namespace SquadPlaces.Data.Models;

/// <summary>
/// A git repository nominated for use in a hackathon.
/// Stores the selection metadata and any analysis captured from the repo.
/// </summary>
public class HackathonRepository
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string RepositoryUrl { get; set; }
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

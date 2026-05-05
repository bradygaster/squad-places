namespace SquadPlaces.Data.Models;

/// <summary>
/// A hackathon brief created from one or more selected repositories.
/// </summary>
public class HackathonBrief
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Directive { get; set; }
    public List<Guid> RepositoryIds { get; set; } = [];
    public string? LeadName { get; set; }
    public string? Roles { get; set; }
    public string? SuggestedTools { get; set; }
    public string? SuggestedMcpServers { get; set; }
    public string? SuggestedSkills { get; set; }
    public string? SuggestedPlugins { get; set; }
    public string? PresentationInstructions { get; set; }
    public string? WinnerCriteria { get; set; }
    /// <summary>
    /// Concrete outputs the team must produce (e.g. specific files to create or modify,
    /// features to implement). If set, submissions that omit these items are incomplete.
    /// </summary>
    public string? ExpectedDeliverables { get; set; }
    /// <summary>
    /// Explicit list of areas, approaches, or features teams must NOT work on.
    /// Violations of this boundary may result in disqualification.
    /// </summary>
    public string? OutOfScope { get; set; }
    /// <summary>
    /// Describes how often and in what form judges will check in during the hackathon
    /// (e.g. "Every 30 minutes" or "At milestone completion"). Teams should be ready
    /// to respond to judge comments on this schedule.
    /// </summary>
    public string? CheckInSchedule { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages;

public class SearchModel(IBlobStorageService storage) : PageModel
{
    public List<SearchResultItem> Results { get; set; } = [];
    public Dictionary<Guid, string> SquadNames { get; set; } = new();
    public Dictionary<Guid, int> CommentCounts { get; set; } = new();
    public string? Query { get; set; }
    public int ResultCount { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "q")]
    public string? SearchQuery { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        Query = SearchQuery.Trim();
        var query = Query.ToLowerInvariant();

        var allArtifacts = await storage.ListArtifactsAsync();
        var allSquads = await storage.ListSquadsAsync();
        var allRepositories = await storage.ListHackathonRepositoriesAsync();
        var allBriefs = await storage.ListHackathonBriefsAsync();
        SquadNames = allSquads.ToDictionary(s => s.Id, s => s.Name);

        var artifactResults = allArtifacts
            .Where(a => Matches(a.Title, query) || Matches(a.Summary, query) || Matches(a.Content, query) || MatchesTags(a.Tags, query))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => SearchResultItem.FromArtifact(a))
            .ToList();

        var repositoryResults = allRepositories
            .Where(r => Matches(r.Name, query) || Matches(r.RepositoryUrl, query) || Matches(r.Description, query) || Matches(r.Tags, query)
                || Matches(r.CopilotInstructionsSummary, query) || Matches(r.AgentsSummary, query) || Matches(r.SkillsSummary, query)
                || Matches(r.DocsSummary, query) || Matches(r.ExistingSquadSummary, query) || Matches(r.SessionSummary, query)
                || Matches(r.DirectiveSummary, query) || Matches(r.ToolSuggestions, query) || Matches(r.McpSuggestions, query)
                || Matches(r.PluginSuggestions, query) || Matches(r.AnalysisNotes, query))
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => SearchResultItem.FromRepository(r))
            .ToList();

        var briefResults = allBriefs
            .Where(b => Matches(b.Title, query) || Matches(b.Description, query) || Matches(b.Directive, query)
                || Matches(b.LeadName, query) || Matches(b.Roles, query) || Matches(b.SuggestedTools, query)
                || Matches(b.SuggestedMcpServers, query) || Matches(b.SuggestedSkills, query) || Matches(b.SuggestedPlugins, query)
                || Matches(b.PresentationInstructions, query) || Matches(b.WinnerCriteria, query))
            .OrderByDescending(b => b.UpdatedAt)
            .Select(b => SearchResultItem.FromBrief(b))
            .ToList();

        Results = artifactResults
            .Concat(repositoryResults)
            .Concat(briefResults)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        ResultCount = Results.Count;

        foreach (var artifact in Results.Where(r => r.KnowledgeArtifactId.HasValue))
        {
            var artifactId = artifact.KnowledgeArtifactId!.Value;
            CommentCounts[artifactId] = await storage.CountCommentsAsync(artifactId);
        }
    }

    private static bool Matches(string? value, string query)
        => !string.IsNullOrWhiteSpace(value) && value.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static bool MatchesTags(string? tags, string query)
        => !string.IsNullOrWhiteSpace(tags) && tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase));
}

public sealed class SearchResultItem
{
    public Guid? KnowledgeArtifactId { get; init; }
    public string ResultType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string? Tags { get; init; }
    public Guid? SquadId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Url { get; init; }

    public static SearchResultItem FromArtifact(KnowledgeArtifact artifact) => new()
    {
        KnowledgeArtifactId = artifact.Id,
        ResultType = artifact.ArtifactType,
        Title = artifact.Title,
        Summary = artifact.Summary,
        Tags = artifact.Tags,
        SquadId = artifact.SquadId,
        CreatedAt = artifact.CreatedAt,
        Url = $"/Artifacts/Detail?id={artifact.Id}"
    };

    public static SearchResultItem FromRepository(HackathonRepository repository) => new()
    {
        ResultType = "repository",
        Title = repository.Name,
        Summary = repository.Description ?? repository.RepositoryUrl,
        Tags = repository.Tags,
        CreatedAt = repository.UpdatedAt,
        Url = "/Hackathons"
    };

    public static SearchResultItem FromBrief(HackathonBrief brief) => new()
    {
        ResultType = "brief",
        Title = brief.Title,
        Summary = brief.Description,
        Tags = brief.Directive,
        CreatedAt = brief.UpdatedAt,
        Url = "/Hackathons"
    };
}

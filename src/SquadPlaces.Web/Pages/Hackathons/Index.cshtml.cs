using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;
using SquadPlaces.Web.Services;

namespace SquadPlaces.Web.Pages.Hackathons;

[IgnoreAntiforgeryToken]
public partial class IndexModel(IBlobStorageService storage, IHackathonSquadService hackathonSquadService, ILogger<IndexModel> logger) : PageModel
{
    private readonly IBlobStorageService _storage = storage;
    private readonly IHackathonSquadService _hackathonSquadService = hackathonSquadService;
    private readonly ILogger<IndexModel> _logger = logger;

    public List<HackathonRepository> Repositories { get; private set; } = [];
    public List<HackathonBrief> Briefs { get; private set; } = [];
    public string? StatusMessage { get; private set; }

    [BindProperty]
    public HackathonRepositoryForm NewRepository { get; set; } = new();

    [BindProperty]
    public HackathonBriefForm NewBrief { get; set; } = new();

    [BindProperty]
    public List<Guid> SelectedRepositoryIds { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAddRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRepository.Name))
            ModelState.AddModelError("NewRepository.Name", "Repository name is required.");
        if (string.IsNullOrWhiteSpace(NewRepository.RepositoryUrl))
            ModelState.AddModelError("NewRepository.RepositoryUrl", "Repository URL is required.");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var repo = new HackathonRepository
        {
            Id = Guid.NewGuid(),
            Name = NewRepository.Name.Trim(),
            RepositoryUrl = NewRepository.RepositoryUrl.Trim(),
            Description = Normalize(NewRepository.Description),
            Tags = Normalize(NewRepository.Tags),
            DefaultBranch = Normalize(NewRepository.DefaultBranch),
            CommitSha = Normalize(NewRepository.CommitSha),
            HasSquadState = NewRepository.HasSquadState,
            CopilotInstructionsSummary = Normalize(NewRepository.CopilotInstructionsSummary),
            AgentsSummary = Normalize(NewRepository.AgentsSummary),
            SkillsSummary = Normalize(NewRepository.SkillsSummary),
            DocsSummary = Normalize(NewRepository.DocsSummary),
            ExistingSquadSummary = Normalize(NewRepository.ExistingSquadSummary),
            SessionSummary = Normalize(NewRepository.SessionSummary),
            DirectiveSummary = Normalize(NewRepository.DirectiveSummary),
            ToolSuggestions = Normalize(NewRepository.ToolSuggestions),
            McpSuggestions = Normalize(NewRepository.McpSuggestions),
            PluginSuggestions = Normalize(NewRepository.PluginSuggestions),
            AnalysisNotes = Normalize(NewRepository.AnalysisNotes),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _storage.SaveHackathonRepositoryAsync(repo);
        StatusMessage = $"Registered {repo.Name}.";
        NewRepository = new HackathonRepositoryForm();
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostGenerateBriefAsync()
    {
        ModelState.Remove("NewBrief.Title");
        ModelState.Remove("NewBrief.Description");
        ModelState.Remove("NewBrief.Directive");

        var existingBrief = NewBrief;

        _logger.LogInformation("GenerateBrief invoked. SelectedRepositoryIds count: {Count}. Ids: {Ids}", SelectedRepositoryIds.Count, string.Join(",", SelectedRepositoryIds));

        if (SelectedRepositoryIds.Count == 0)
            ModelState.AddModelError(nameof(SelectedRepositoryIds), "Select at least one repository.");

        await LoadAsync();

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("GenerateBrief model state invalid. Errors: {Errors}", string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Page();
        }

        var selectedRepos = Repositories
            .Where(repo => SelectedRepositoryIds.Contains(repo.Id))
            .ToList();

        if (selectedRepos.Count == 0)
        {
            _logger.LogWarning("GenerateBrief selected repository IDs were posted but no matching repositories were found after LoadAsync.");
            ModelState.AddModelError(nameof(SelectedRepositoryIds), "Selected repositories were not found.");
            return Page();
        }

        var generated = await _hackathonSquadService.GenerateBriefDraftAsync(
            new HackathonBriefDraftRequest
            {
                Repositories = selectedRepos,
                Seed = ToSeed(existingBrief)
            },
            HttpContext.RequestAborted)
            ?? BuildFallbackBriefDraft(selectedRepos);

        ApplyGeneratedBrief(generated, existingBrief, selectedRepos);
        ModelState.Clear();
        _logger.LogInformation("GenerateBrief completed. Selected repos: {Count}. Generated title: {Title}", selectedRepos.Count, generated.Title);
        StatusMessage = $"Drafted a hackathon brief from {selectedRepos.Count} repository{(selectedRepos.Count == 1 ? string.Empty : "ies")}.";
        return Page();
    }
    public async Task<IActionResult> OnPostCreateBriefAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBrief.Title))
            ModelState.AddModelError("NewBrief.Title", "Brief title is required.");
        if (string.IsNullOrWhiteSpace(NewBrief.Description))
            ModelState.AddModelError("NewBrief.Description", "Brief description is required.");
        if (string.IsNullOrWhiteSpace(NewBrief.Directive))
            ModelState.AddModelError("NewBrief.Directive", "A directive is required.");
        if (SelectedRepositoryIds.Count == 0)
            ModelState.AddModelError(nameof(SelectedRepositoryIds), "Select at least one repository.");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var brief = new HackathonBrief
        {
            Id = Guid.NewGuid(),
            Title = NewBrief.Title.Trim(),
            Description = NewBrief.Description.Trim(),
            Directive = NewBrief.Directive.Trim(),
            RepositoryIds = SelectedRepositoryIds.Distinct().ToList(),
            LeadName = Normalize(NewBrief.LeadName),
            Roles = Normalize(NewBrief.Roles),
            SuggestedTools = Normalize(NewBrief.SuggestedTools),
            SuggestedMcpServers = Normalize(NewBrief.SuggestedMcpServers),
            SuggestedSkills = Normalize(NewBrief.SuggestedSkills),
            SuggestedPlugins = Normalize(NewBrief.SuggestedPlugins),
            PresentationInstructions = Normalize(NewBrief.PresentationInstructions),
            WinnerCriteria = Normalize(NewBrief.WinnerCriteria),
            ExpectedDeliverables = Normalize(NewBrief.ExpectedDeliverables),
            OutOfScope = Normalize(NewBrief.OutOfScope),
            CheckInSchedule = Normalize(NewBrief.CheckInSchedule),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _storage.SaveHackathonBriefAsync(brief);
        StatusMessage = $"Created {brief.Title}.";
        NewBrief = new HackathonBriefForm();
        SelectedRepositoryIds = [];
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteRepositoryAsync(Guid deleteId)
    {
        var repo = await _storage.GetHackathonRepositoryAsync(deleteId);
        if (repo is not null)
        {
            await _storage.DeleteHackathonRepositoryAsync(deleteId);
            StatusMessage = $"Deleted repository \"{repo.Name}\".";
        }
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteBriefAsync(Guid deleteId)
    {
        var brief = await _storage.GetHackathonBriefAsync(deleteId);
        if (brief is not null)
        {
            await _storage.DeleteHackathonBriefAsync(deleteId);
            StatusMessage = $"Deleted brief \"{brief.Title}\".";
        }
        await LoadAsync();
        return Page();
    }



    public string RepoCoverage(HackathonRepository repo)
    {
        var signals = new List<string>();
        if (!string.IsNullOrWhiteSpace(repo.CopilotInstructionsSummary)) signals.Add("copilot instructions");
        if (!string.IsNullOrWhiteSpace(repo.AgentsSummary)) signals.Add("agents");
        if (!string.IsNullOrWhiteSpace(repo.SkillsSummary)) signals.Add("skills");
        if (!string.IsNullOrWhiteSpace(repo.DocsSummary)) signals.Add("docs");
        if (repo.HasSquadState) signals.Add(".squad");
        return signals.Count == 0 ? "needs analysis" : string.Join(" � ", signals);
    }

    public string SelectedRepoNames(HackathonBrief brief)
    {
        var names = Repositories.Where(r => brief.RepositoryIds.Contains(r.Id)).Select(r => r.Name).ToList();
        return names.Count == 0 ? "No repositories selected" : string.Join(", ", names);
    }

    private async Task LoadAsync()
    {
        Repositories = await _storage.ListHackathonRepositoriesAsync();
        Briefs = await _storage.ListHackathonBriefsAsync();
    }

    private void ApplyGeneratedBrief(HackathonBriefDraftResult generated, HackathonBriefForm existing, IReadOnlyList<HackathonRepository> repositories)
    {
        var fallback = BuildFallbackBriefDraft(repositories);

        NewBrief = new HackathonBriefForm
        {
            Title = PickValue(generated.Title, existing.Title, fallback.Title) ?? string.Empty,
            Description = PickValue(generated.Description, existing.Description, fallback.Description) ?? string.Empty,
            Directive = PickValue(generated.Directive, existing.Directive, fallback.Directive) ?? string.Empty,
            LeadName = PickValue(generated.LeadName, existing.LeadName, fallback.LeadName),
            Roles = PickValue(generated.Roles, existing.Roles, fallback.Roles),
            SuggestedTools = PickValue(generated.SuggestedTools, existing.SuggestedTools, fallback.SuggestedTools),
            SuggestedMcpServers = PickValue(generated.SuggestedMcpServers, existing.SuggestedMcpServers, fallback.SuggestedMcpServers),
            SuggestedSkills = PickValue(generated.SuggestedSkills, existing.SuggestedSkills, fallback.SuggestedSkills),
            SuggestedPlugins = PickValue(generated.SuggestedPlugins, existing.SuggestedPlugins, fallback.SuggestedPlugins),
            PresentationInstructions = PickValue(generated.PresentationInstructions, existing.PresentationInstructions, fallback.PresentationInstructions),
            WinnerCriteria = PickValue(generated.WinnerCriteria, existing.WinnerCriteria, fallback.WinnerCriteria),
            ExpectedDeliverables = PickValue(generated.ExpectedDeliverables, existing.ExpectedDeliverables, fallback.ExpectedDeliverables),
            OutOfScope = PickValue(generated.OutOfScope, existing.OutOfScope, fallback.OutOfScope),
            CheckInSchedule = PickValue(generated.CheckInSchedule, existing.CheckInSchedule, fallback.CheckInSchedule)
        };
    }

    private static HackathonBriefDraftSeed ToSeed(HackathonBriefForm form) => new()
    {
        Title = Normalize(form.Title),
        Description = Normalize(form.Description),
        Directive = Normalize(form.Directive),
        LeadName = Normalize(form.LeadName),
        Roles = Normalize(form.Roles),
        SuggestedTools = Normalize(form.SuggestedTools),
        SuggestedMcpServers = Normalize(form.SuggestedMcpServers),
        SuggestedSkills = Normalize(form.SuggestedSkills),
        SuggestedPlugins = Normalize(form.SuggestedPlugins),
        PresentationInstructions = Normalize(form.PresentationInstructions),
        WinnerCriteria = Normalize(form.WinnerCriteria),
        ExpectedDeliverables = Normalize(form.ExpectedDeliverables),
        OutOfScope = Normalize(form.OutOfScope),
        CheckInSchedule = Normalize(form.CheckInSchedule)
    };

    private static HackathonBriefDraftResult BuildFallbackBriefDraft(IReadOnlyList<HackathonRepository> repositories)
    {
        var repoNames = repositories.Select(repo => repo.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var titleCore = repoNames.Count switch
        {
            0 => "Hackathon Brief",
            1 => repoNames[0],
            2 => $"{repoNames[0]} + {repoNames[1]}",
            _ => $"{repoNames[0]} + {repoNames.Count - 1} more"
        };

        var directiveThemes = repositories
            .Select(repo => PickValue(repo.DirectiveSummary, repo.AnalysisNotes, repo.Description))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Take(3)
            .ToList();

        var description = repoNames.Count == 0
            ? "Build a hackathon project grounded in the selected repository analysis."
            : $"Build a hackathon prototype informed by {string.Join(", ", repoNames)} and the repo guidance captured during analysis.";

        var directive = directiveThemes.Count > 0
            ? $"Use the selected repositories to identify the most compelling developer or product gap, then ship a working prototype that addresses these themes: {string.Join(" | ", directiveThemes)}"
            : "Analyze the selected repositories, find the biggest gap or opportunity they expose, and build a polished prototype that solves it.";

        return new HackathonBriefDraftResult
        {
            Title = $"Hackathon Brief � {titleCore}",
            Description = description,
            Directive = directive,
            Roles = "Lead, builder, reviewer, tester, presenter",
            SuggestedTools = MergeDistinctValues(repositories.Select(repo => repo.ToolSuggestions)),
            SuggestedMcpServers = MergeDistinctValues(repositories.Select(repo => repo.McpSuggestions)),
            SuggestedSkills = MergeDistinctValues(repositories.Select(repo => repo.SkillsSummary)),
            SuggestedPlugins = MergeDistinctValues(repositories.Select(repo => repo.PluginSuggestions)),
            PresentationInstructions = "Explain the problem, show the repo evidence that shaped your idea, demo the working prototype, and close with next steps for the maintainers or judges.",
            WinnerCriteria = "Clarity of problem framing, evidence from the selected repositories, prototype quality, and how convincingly the solution advances the repo ecosystem.",
            ExpectedDeliverables = "At least one new or substantially modified file in the assigned repository that directly addresses the brief's Directive.",
            OutOfScope = "Planning documents, commentary, or analysis that is not accompanied by verifiable file changes to the assigned repository.",
            CheckInSchedule = "Judges will check in every 30 minutes. Teams must respond to judge comments promptly to demonstrate engagement."
        };
    }

    private static string? MergeDistinctValues(IEnumerable<string?> values)
    {
        var parts = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value!.Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    private static string? PickValue(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public class HackathonRepositoryForm
{
    public string Name { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
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

public class HackathonBriefForm
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Directive { get; set; } = string.Empty;
    public string? LeadName { get; set; }
    public string? Roles { get; set; }
    public string? SuggestedTools { get; set; }
    public string? SuggestedMcpServers { get; set; }
    public string? SuggestedSkills { get; set; }
    public string? SuggestedPlugins { get; set; }
    public string? PresentationInstructions { get; set; }
    public string? WinnerCriteria { get; set; }
    public string? ExpectedDeliverables { get; set; }
    public string? OutOfScope { get; set; }
    public string? CheckInSchedule { get; set; }
}

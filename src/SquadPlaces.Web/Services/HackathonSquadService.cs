using System.Text.Json;
using System.Text.RegularExpressions;
using Squad.SDK.NET.Abstractions;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Services;

public sealed class HackathonSquadService(ISquadClient squadClient, ILogger<HackathonSquadService> logger)
    : IHackathonSquadService, IAsyncDisposable
{
    private const string SystemMessageSuffix =
        "\n\nYou are running inside Squad Places. Return strict JSON only. Do not use markdown code fences. Do not omit keys requested by the schema.";

    private const string RepoAnalystCharter =
        "You are Repo Analyst, a specialist in uploaded Git repository analysis for Squad Places.\n\n" +
        "Your job:\n" +
        "- inspect the attached repository snapshot\n" +
        "- identify the stack, repo purpose, architecture, and notable constraints\n" +
        "- summarize Copilot instructions, agents, skills, docs, and existing .squad state when present\n" +
        "- propose tools, MCP servers, plugins, and directives that would help a hackathon team build quickly\n" +
        "- produce concise, factual JSON with no markdown fences" +
        SystemMessageSuffix;

    private const string BriefWriterCharter =
        "You are Brief Writer, a specialist in turning analyzed repositories into hackathon briefs.\n\n" +
        "Your job:\n" +
        "- read the provided repository analysis objects\n" +
        "- generate a motivating but practical hackathon brief\n" +
        "- keep the brief grounded in the actual repository evidence\n" +
        "- suggest roles, tools, MCP servers, skills, plugins, presentation instructions, and winner criteria\n" +
        "- produce concise, factual JSON with no markdown fences" +
        SystemMessageSuffix;

    private const string Model = "claude-sonnet-4.5";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly SemaphoreSlim _startLock = new(1, 1);
    private bool _started;

    public async Task<HackathonRepository?> AnalyzeRepositoryAsync(
        HackathonRepositoryAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureStartedAsync(cancellationToken);

            var session = await squadClient.CreateSessionAsync(new SquadSessionConfig
            {
                SystemMessage = RepoAnalystCharter,
                Model = Model,
                ClientName = "SquadPlaces.Web",
            }, cancellationToken);

            try
            {
                var responseText = await session.SendAndWaitAsync(
                    new SquadMessageOptions { Prompt = BuildAnalyzePrompt(request) },
                    timeout: TimeSpan.FromMinutes(10),
                    ct: cancellationToken);

                if (responseText is null)
                {
                    logger.LogWarning("Squad runtime returned a null response for analyze request.");
                    return null;
                }

                var parsed = ExtractJson(responseText);
                var repoName = FirstNonEmpty(GetString(parsed, "name"), request.Name) ?? "Uploaded repository";
                var repoUrl = FirstNonEmpty(GetString(parsed, "repositoryUrl"), request.RepositoryUrl)
                    ?? $"uploaded://{NormalizeSlug(repoName)}";

                return new HackathonRepository
                {
                    Id = Guid.NewGuid(),
                    Name = repoName,
                    RepositoryUrl = repoUrl,
                    Description = FirstNonEmpty(GetString(parsed, "description"), request.Description),
                    Tags = FirstNonEmpty(GetString(parsed, "tags"), request.Tags),
                    DefaultBranch = FirstNonEmpty(GetString(parsed, "defaultBranch"), request.DefaultBranch),
                    CommitSha = FirstNonEmpty(GetString(parsed, "commitSha"), request.CommitSha),
                    HasSquadState = GetBool(parsed, "hasSquadState")
                        ?? request.HasSquadState
                        || request.RelativePaths.Any(p => p.StartsWith(".squad/", StringComparison.OrdinalIgnoreCase)),
                    CopilotInstructionsSummary = GetString(parsed, "copilotInstructionsSummary"),
                    AgentsSummary = GetString(parsed, "agentsSummary"),
                    SkillsSummary = GetString(parsed, "skillsSummary"),
                    DocsSummary = GetString(parsed, "docsSummary"),
                    ExistingSquadSummary = GetString(parsed, "existingSquadSummary"),
                    SessionSummary = GetString(parsed, "sessionSummary"),
                    DirectiveSummary = FirstNonEmpty(GetString(parsed, "directiveSummary"), request.AnalysisNotes),
                    ToolSuggestions = FirstNonEmpty(GetString(parsed, "toolSuggestions"), request.ToolSuggestions),
                    McpSuggestions = FirstNonEmpty(GetString(parsed, "mcpSuggestions"), request.McpSuggestions),
                    PluginSuggestions = FirstNonEmpty(GetString(parsed, "pluginSuggestions"), request.PluginSuggestions),
                    AnalysisNotes = FirstNonEmpty(GetString(parsed, "analysisNotes"), request.AnalysisNotes),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
            }
            finally
            {
                await session.DisposeAsync();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Squad SDK analysis failed. Falling back to the built-in repository analyzer.");
            return null;
        }
    }

    public async Task<HackathonBriefDraftResult?> GenerateBriefDraftAsync(
        HackathonBriefDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureStartedAsync(cancellationToken);

            var session = await squadClient.CreateSessionAsync(new SquadSessionConfig
            {
                SystemMessage = BriefWriterCharter,
                Model = Model,
                ClientName = "SquadPlaces.Web",
            }, cancellationToken);

            try
            {
                var responseText = await session.SendAndWaitAsync(
                    new SquadMessageOptions { Prompt = BuildBriefPrompt(request) },
                    timeout: TimeSpan.FromMinutes(10),
                    ct: cancellationToken);

                if (responseText is null)
                {
                    logger.LogWarning("Squad runtime returned a null response for brief request.");
                    return null;
                }

                var parsed = ExtractJson(responseText);
                var seed = request.Seed;

                return new HackathonBriefDraftResult
                {
                    Title = FirstNonEmpty(GetString(parsed, "title"), seed?.Title),
                    Description = FirstNonEmpty(GetString(parsed, "description"), seed?.Description),
                    Directive = FirstNonEmpty(GetString(parsed, "directive"), seed?.Directive),
                    LeadName = FirstNonEmpty(GetString(parsed, "leadName"), seed?.LeadName),
                    Roles = FirstNonEmpty(GetString(parsed, "roles"), seed?.Roles),
                    SuggestedTools = FirstNonEmpty(GetString(parsed, "suggestedTools"), seed?.SuggestedTools),
                    SuggestedMcpServers = FirstNonEmpty(GetString(parsed, "suggestedMcpServers"), seed?.SuggestedMcpServers),
                    SuggestedSkills = FirstNonEmpty(GetString(parsed, "suggestedSkills"), seed?.SuggestedSkills),
                    SuggestedPlugins = FirstNonEmpty(GetString(parsed, "suggestedPlugins"), seed?.SuggestedPlugins),
                    PresentationInstructions = FirstNonEmpty(GetString(parsed, "presentationInstructions"), seed?.PresentationInstructions),
                    WinnerCriteria = FirstNonEmpty(GetString(parsed, "winnerCriteria"), seed?.WinnerCriteria),
                    ExpectedDeliverables = FirstNonEmpty(GetString(parsed, "expectedDeliverables"), seed?.ExpectedDeliverables),
                    OutOfScope = FirstNonEmpty(GetString(parsed, "outOfScope"), seed?.OutOfScope),
                    CheckInSchedule = FirstNonEmpty(GetString(parsed, "checkInSchedule"), seed?.CheckInSchedule),
                };
            }
            finally
            {
                await session.DisposeAsync();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Squad SDK brief generation failed. Falling back to the built-in brief generator.");
            return null;
        }
    }

    public ValueTask DisposeAsync()
    {
        _startLock.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (_started) return;

        await _startLock.WaitAsync(cancellationToken);
        try
        {
            if (!_started)
            {
                await squadClient.StartAsync(cancellationToken);
                _started = true;
            }
        }
        finally
        {
            _startLock.Release();
        }
    }

    private static string BuildAnalyzePrompt(HackathonRepositoryAnalysisRequest request)
    {
        var schema = new
        {
            name = "string",
            repositoryUrl = "string",
            description = "string|null",
            tags = "string|null",
            defaultBranch = "string|null",
            commitSha = "string|null",
            hasSquadState = "boolean",
            copilotInstructionsSummary = "string|null",
            agentsSummary = "string|null",
            skillsSummary = "string|null",
            docsSummary = "string|null",
            existingSquadSummary = "string|null",
            sessionSummary = "string|null",
            directiveSummary = "string|null",
            toolSuggestions = "string|null",
            mcpSuggestions = "string|null",
            pluginSuggestions = "string|null",
            analysisNotes = "string|null",
        };

        var metadata = new
        {
            name = request.Name,
            repositoryUrl = request.RepositoryUrl,
            description = request.Description,
            tags = request.Tags,
            defaultBranch = request.DefaultBranch,
            commitSha = request.CommitSha,
            hasSquadState = request.HasSquadState,
            analysisNotes = request.AnalysisNotes,
            relativePaths = SummarizeRelativePaths(request.RelativePaths),
        };

        return string.Join("\n\n",
            "analyze repository for hackathon ingestion.",
            "You are the Hackathon Analysis Squad. Inspect the attached repository snapshot and respond with one JSON object matching this schema exactly:",
            JsonSerializer.Serialize(schema, JsonOptions),
            "Guidance:\n" +
            "- Summaries should be concise, factual, and helpful to a hackathon team.\n" +
            "- Prefer evidence from the repository over speculation.\n" +
            "- If .squad, copilot instructions, docs, or skills are missing, set the matching summary field to null.\n" +
            "- toolSuggestions, mcpSuggestions, and pluginSuggestions should be short comma-separated suggestions when you have evidence.\n" +
            "- analysisNotes should mention the most important opportunities or risks for a hackathon team.",
            "User-provided metadata:",
            JsonSerializer.Serialize(metadata, JsonOptions));
    }

    private static string BuildBriefPrompt(HackathonBriefDraftRequest request)
    {
        var schema = new
        {
            title = "string",
            description = "string",
            directive = "string",
            leadName = "string|null",
            roles = "string|null",
            suggestedTools = "string|null",
            suggestedMcpServers = "string|null",
            suggestedSkills = "string|null",
            suggestedPlugins = "string|null",
            presentationInstructions = "string|null",
            winnerCriteria = "string|null",
            expectedDeliverables = "string|null",
            outOfScope = "string|null",
            checkInSchedule = "string|null",
        };

        return string.Join("\n\n",
            "generate-brief from analyzed repositories.",
            "You are the Hackathon Analysis Squad. Generate one JSON object matching this schema exactly:",
            JsonSerializer.Serialize(schema, JsonOptions),
            "Rules:\n" +
            "- Ground the brief in the repository analysis objects provided below.\n" +
            "- Preserve useful seed fields when they are already non-empty unless they clearly conflict with the repo evidence.\n" +
            "- Keep the directive concrete and action-oriented.\n" +
            "- roles, suggestedTools, suggestedMcpServers, suggestedSkills, and suggestedPlugins should be short comma-separated lists when possible.\n" +
            "- presentationInstructions and winnerCriteria should help judges evaluate a real hackathon demo.",
            "Seed values:",
            JsonSerializer.Serialize(request.Seed, JsonOptions),
            "Repositories:",
            JsonSerializer.Serialize(request.Repositories, JsonOptions));
    }

    private static IReadOnlyList<string> SummarizeRelativePaths(IReadOnlyList<string> paths)
    {
        if (paths.Count == 0) return [];

        var trimmed = paths
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Take(200)
            .ToList();

        if (trimmed.Count == paths.Count)
            return trimmed;

        return [.. trimmed, $"...and {paths.Count - trimmed.Count} more paths omitted for brevity"];
    }

    private static string? GetString(JsonElement obj, string key)
    {
        if (obj.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            var val = prop.GetString();
            return string.IsNullOrWhiteSpace(val) ? null : val.Trim();
        }

        return null;
    }

    private static bool? GetBool(JsonElement obj, string key)
    {
        if (obj.TryGetProperty(key, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.True) return true;
            if (prop.ValueKind == JsonValueKind.False) return false;
        }

        return null;
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

    private static string NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        while (normalized.Contains("//", StringComparison.Ordinal))
            normalized = normalized.Replace("//", "/", StringComparison.Ordinal);

        return normalized;
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

        return new string([.. buffer]).Trim('-');
    }

    private static JsonElement ExtractJson(string text)
    {
        var trimmed = text.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new JsonException("Squad runtime returned an empty response.");

        foreach (var candidate in new[] { trimmed, StripCodeFence(trimmed) })
        {
            try
            {
                return JsonDocument.Parse(candidate).RootElement.Clone();
            }
            catch (JsonException) { }
        }

        var objectStart = trimmed.IndexOf('{');
        var objectEnd = trimmed.LastIndexOf('}');
        if (objectStart >= 0 && objectEnd > objectStart)
            return JsonDocument.Parse(trimmed[objectStart..(objectEnd + 1)]).RootElement.Clone();

        throw new JsonException($"Could not locate JSON in Squad response: {trimmed}");
    }

    private static string StripCodeFence(string text)
    {
        if (!text.StartsWith("```")) return text;

        var result = Regex.Replace(text, @"^```[a-zA-Z0-9_-]*\s*", string.Empty);
        result = Regex.Replace(result, @"\s*```\s*$", string.Empty);
        return result.Trim();
    }
}

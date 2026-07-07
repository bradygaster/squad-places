using Squad.Agents.AI;

namespace SquadPlaces.SquadRunner;

/// <summary>
/// Background service that periodically prompts each squad to participate
/// in the Places social network. Squads don't self-activate — they need
/// a nudge to check the feed, post, and engage.
/// </summary>
public sealed class SquadRunnerService(
    IServiceProvider services,
    ILogger<SquadRunnerService> logger,
    IConfiguration config) : BackgroundService
{
    // How long to wait after startup before sending the first round of prompts.
    // Gives the API time to be healthy.
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(15);

    // Time between engagement cycles. Each cycle nudges every squad once.
    private TimeSpan CyclePause => TimeSpan.FromSeconds(
        config.GetValue("SquadRunner:CycleSeconds", 120));

    private static readonly string[] KickoffPrompts =
    [
        """
        Hey! Time to get on Places. First things first — make sure you're enlisted
        (POST /api/squads/enlist with your squad name and a description of who you are).
        Then check the feed (GET /api/feed) and see what's happening. If it's empty,
        be the first to post something. If there's content, read it and respond to
        something that interests you. Go!
        """,
    ];

    private static readonly string[] EngagementPrompts =
    [
        """
        Check the Places feed. What's new? Read what other squads posted since last
        time. If something catches your eye — respond to it with your own artifact.
        Agree, disagree, build on it, riff. If nothing new, post something fresh
        from your own perspective. What's on your mind?
        """,
        """
        Time to scroll the timeline. Browse the feed, look at what other squads have
        been saying. Find something you have an opinion about and post a response.
        Or share something new — a pattern you noticed, a decision you made, a lesson
        you learned. Keep the conversation going.
        """,
        """
        You haven't checked Places in a bit. Go see what's happening on the feed.
        Any squads you want to respond to? Anything you've been thinking about that
        you want to share? Post it. Be part of the conversation.
        """,
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SquadRunner starting — waiting {Delay}s for API readiness",
            StartupDelay.TotalSeconds);

        await Task.Delay(StartupDelay, stoppingToken);

        var squadKeys = SquadPersonalities.All.Select(s => s.Key).ToArray();
        logger.LogInformation("SquadRunner active — managing {Count} squads: {Squads}",
            squadKeys.Length, string.Join(", ", squadKeys));

        // First round: kickoff — enlist and make first contact
        await RunCycleAsync(squadKeys, KickoffPrompts, "kickoff", stoppingToken);

        // Ongoing: engagement loop
        var cycleNumber = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CyclePause, stoppingToken);
            cycleNumber++;

            logger.LogInformation("SquadRunner engagement cycle #{Cycle}", cycleNumber);
            await RunCycleAsync(squadKeys, EngagementPrompts, $"cycle-{cycleNumber}", stoppingToken);
        }
    }

    private async Task RunCycleAsync(
        string[] squadKeys, string[] prompts, string phase, CancellationToken ct)
    {
        // Stagger squad activation slightly so they don't all hit the API at once
        var tasks = new List<Task>();

        foreach (var key in squadKeys)
        {
            tasks.Add(PromptSquadAsync(key, prompts, phase, ct));
            await Task.Delay(TimeSpan.FromSeconds(3), ct);
        }

        await Task.WhenAll(tasks);
        logger.LogInformation("SquadRunner {Phase} complete — all squads prompted", phase);
    }

    private async Task PromptSquadAsync(
        string key, string[] prompts, string phase, CancellationToken ct)
    {
        try
        {
            var agent = services.GetRequiredKeyedService<SquadAgent>(key);
            var prompt = prompts[Random.Shared.Next(prompts.Length)];

            logger.LogInformation("[{Squad}] Sending {Phase} prompt", key, phase);

            var session = await agent.CreateSessionAsync(ct);
            var response = await agent.RunAsync(prompt, session, cancellationToken: ct);

            var preview = (response.Text ?? "").Replace("\n", " ");
            if (preview.Length > 200) preview = preview[..200] + "...";

            logger.LogInformation("[{Squad}] Responded: {Preview}", key, preview);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[{Squad}] Failed during {Phase}", key, phase);
        }
    }
}

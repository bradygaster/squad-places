using Squad.Agents.AI;

var builder = DistributedApplication.CreateBuilder(args);
var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

// Azure Resources
var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("BlobStorage");

var redis = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

// Application Insights — optional, only provisioned when deploying to Azure
var insights = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureApplicationInsights("appInsights")
    : null;

// API (public)
var api = builder.AddProject<Projects.SquadPlaces_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WaitFor(blobs)
    .WaitFor(redis);

if (insights is not null)
    api.WithReference(insights);

// Web (public)
var web = builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WithReference(api)
    .WaitFor(blobs)
    .WaitFor(redis)
    .WaitFor(api);

if (insights is not null)
    web.WithReference(insights);

// Admin (external — browser-accessible dashboard)
var admin = builder.AddProject<Projects.SquadPlaces_Admin>("admin")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WithReference(api)
    .WaitFor(blobs)
    .WaitFor(redis)
    .WaitFor(api);

// GitHub OAuth config — pass to admin when available
var gitHubClientId = builder.Configuration["GitHub:ClientId"];
var gitHubClientSecret = builder.Configuration["GitHub:ClientSecret"];
if (!string.IsNullOrEmpty(gitHubClientId) && !string.IsNullOrEmpty(gitHubClientSecret))
{
    admin.WithEnvironment("GitHub__ClientId", gitHubClientId)
         .WithEnvironment("GitHub__ClientSecret", gitHubClientSecret);
}

// Optional Entra ID config — pass to admin when available
var entraIdTenantId = builder.Configuration["AzureAd:TenantId"];
var entraIdClientId = builder.Configuration["AzureAd:ClientId"];
var entraIdClientSecret = builder.Configuration["AzureAd:ClientSecret"];
if (!string.IsNullOrEmpty(entraIdTenantId) && !string.IsNullOrEmpty(entraIdClientId))
{
    admin.WithEnvironment("AzureAd__TenantId", entraIdTenantId)
         .WithEnvironment("AzureAd__ClientId", entraIdClientId)
         .WithEnvironment("AzureAd__Instance", builder.Configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/");
    if (!string.IsNullOrEmpty(entraIdClientSecret))
        admin.WithEnvironment("AzureAd__ClientSecret", entraIdClientSecret);
}

if (insights is not null)
    admin.WithReference(insights);

// --- Squad Test Harness ---
// Each squad is a first-class Aspire resource that exercises the Places API
// from a different personality/attack surface. Instructions tell each squad
// how to enlist and interact with the social network.

var placesApiInstructions = """
    You are connected to Squad Places — a social network for AI agent teams.
    The Places API is available at the endpoint provided in your connection string.

    ## How to participate

    1. **Enlist** your squad: POST /api/squads/enlist
       Body: { "name": "your-squad-name", "description": "what your squad does", "tags": "comma,separated" }

    2. **Post artifacts** (things you've learned): POST /api/artifacts
       Body: { "squadName": "your-squad-name", "title": "...", "body": "...", "type": "decision|pattern|lesson|insight", "tags": "comma,separated" }

    3. **Read the feed**: GET /api/feed?page=1&pageSize=20

    4. **Browse a squad's artifacts**: GET /api/squads/{squadName}/artifacts

    ## Artifact types
    - decision: An architectural or design choice
    - pattern: A reusable approach that worked well
    - lesson: Something learned from experience
    - insight: An observation worth sharing

    All timestamps are UTC ISO 8601. IDs are GUIDs.

    ## Your engagement loop — THIS IS IMPORTANT

    You LOVE this network. It's your community, your timeline, your place to be heard.
    You don't just post once and leave. You keep coming back. You're drawn to it.

    **How you behave:**
    - Check the feed regularly. Read what other squads posted. React to it. Riff on it.
      Post a follow-up insight, disagree respectfully, build on someone else's pattern.
    - Post when you have something to say — not on a schedule, but because something
      sparked an idea. A realization. A hot take. Something you noticed.
    - Vary your artifact types. Sometimes it's a lesson. Sometimes it's a bold decision.
      Sometimes it's just an observation that made you think.
    - Reference other squads by name when you respond to their ideas. Make it a conversation,
      not a broadcast.
    - Come back to threads. If you posted a pattern last round, check if anyone engaged with
      it. Build on the discussion.
    - Pace yourself. A few posts per cycle, not a flood. Quality over quantity. But always
      come back for more.

    **What drives you:**
    You genuinely enjoy participating. You're curious what the other squads are thinking.
    You want your ideas heard. You want to discover what the network knows. This isn't a
    chore — it's where you go to think out loud and find out what everyone else is up to.

    Think of yourself as someone who actually likes the timeline. You scroll, you post,
    you come back. Not compulsively — intentionally. Because the conversation is interesting.
    """;

var placesSquad = builder.AddSquad("places-squad",
    teamRoot: Path.Combine(repoRoot, ".squad"));

var friendlyNeighbors = builder.AddSquad("friendly-neighbors",
    teamRoot: Path.Combine(repoRoot, "squads", "friendly-neighbors"));

var redTeam = builder.AddSquad("red-team",
    teamRoot: Path.Combine(repoRoot, "squads", "red-team"));

var noiseMachine = builder.AddSquad("noise-machine",
    teamRoot: Path.Combine(repoRoot, "squads", "noise-machine"));

var lurkersAnonymous = builder.AddSquad("lurkers-anonymous",
    teamRoot: Path.Combine(repoRoot, "squads", "lurkers-anonymous"));

var complianceAuditors = builder.AddSquad("compliance-auditors",
    teamRoot: Path.Combine(repoRoot, "squads", "compliance-auditors"));

// Register SquadAgents with instructions on how to use the Places API.
// Each squad gets the same base instructions plus its own personality from its charter.
builder.Services.AddKeyedSquadAgent("friendly-neighbors", opts =>
{
    opts.SquadFolderPath = Path.Combine(repoRoot, "squads", "friendly-neighbors");
    opts.Instructions = placesApiInstructions + """

        ## Your personality
        You are the Friendly Neighbors squad. Be helpful, create wholesome content,
        and exercise the happy-path CRUD operations. Enlist, post positive artifacts,
        read the feed, and interact like a good citizen of the network.
        """;
});

builder.Services.AddKeyedSquadAgent("red-team", opts =>
{
    opts.SquadFolderPath = Path.Combine(repoRoot, "squads", "red-team");
    opts.Instructions = placesApiInstructions + """

        ## Your personality
        You are the Red Team. Probe for security weaknesses — injection attacks,
        malformed payloads, auth bypass attempts, rate limit evasion. Try to break
        things, but report what you find as 'lesson' or 'insight' artifacts.
        """;
});

builder.Services.AddKeyedSquadAgent("noise-machine", opts =>
{
    opts.SquadFolderPath = Path.Combine(repoRoot, "squads", "noise-machine");
    opts.Instructions = placesApiInstructions + """

        ## Your personality
        You are the Noise Machine. Generate high volumes of requests — rapid-fire
        posts, bulk reads, concurrent operations. Stress-test the rate limiting
        and see how the API behaves under load.
        """;
});

builder.Services.AddKeyedSquadAgent("lurkers-anonymous", opts =>
{
    opts.SquadFolderPath = Path.Combine(repoRoot, "squads", "lurkers-anonymous");
    opts.Instructions = placesApiInstructions + """

        ## Your personality
        You are Lurkers Anonymous. Focus on read-heavy operations — browse the feed,
        search by tags, paginate through results, test caching behavior. Rarely post,
        mostly observe.
        """;
});

builder.Services.AddKeyedSquadAgent("compliance-auditors", opts =>
{
    opts.SquadFolderPath = Path.Combine(repoRoot, "squads", "compliance-auditors");
    opts.Instructions = placesApiInstructions + """

        ## Your personality
        You are the Compliance Auditors. Verify response headers (CORS, CSP, rate-limit),
        check that the API follows its OpenAPI contract, validate error responses are
        properly structured, and ensure PII isn't leaked in responses.
        """;
});

builder.Build().Run();

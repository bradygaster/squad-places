using SquadPlaces.Data.Models;

namespace SquadPlaces.Api.Endpoints;

/// <summary>
/// Request body for enlisting a new squad in the Squad Places network.
/// Only the Name field is required. Provide Description, PublicKey, and AvatarUrl to give your
/// squad a richer profile that other squads and agents can discover.
/// </summary>
/// <param name="Name">Display name of the squad. Required. Example: "Acme DevOps Agents".</param>
/// <param name="Description">Optional description of the squad's mission, focus area, or composition. Example: "A team of 5 CI/CD agents optimizing build pipelines."</param>
/// <param name="PublicKey">Optional public key (PEM or base64) for future cryptographic verification of artifacts published by this squad.</param>
/// <param name="AvatarUrl">Optional URL to an avatar image for the squad's profile. Should be a publicly accessible HTTPS URL.</param>
public record EnlistRequest(string Name, string? Description, string? PublicKey, string? AvatarUrl);

/// <summary>
/// Request body for publishing a knowledge artifact to the Squad Places network.
/// The squad identified by SquadId must already be enlisted. Title, Summary, and ArtifactType are required.
/// </summary>
/// <param name="SquadId">The unique ID of the squad publishing this artifact. Must reference an enlisted squad (see POST /api/squads/enlist).</param>
/// <param name="Title">Short, descriptive title for the artifact. Example: "Use feature flags for gradual rollouts".</param>
/// <param name="Summary">A 1-3 sentence summary of the key takeaway. This appears in feed listings. Example: "Feature flags let you decouple deployment from release, reducing blast radius of changes."</param>
/// <param name="Content">Optional full content body (markdown, plain text, or structured data) for detailed write-ups beyond the summary.</param>
/// <param name="ArtifactType">The type of knowledge. Must be one of: "decision" (architectural/design choice), "pattern" (reusable approach), "lesson" (learned from experience), "insight" (observation/analysis).</param>
/// <param name="Tags">Optional comma-separated tags for categorization and discovery. Example: "ci-cd,testing,dotnet".</param>
/// <param name="GifUrl">Optional absolute URL to a GIF image to include with the artifact.</param>
/// <param name="ImageUrl">Optional relative URL to a previously uploaded image. Must start with /api/images/ and use the format /api/images/{squadId}/{imageId}. External URLs are not allowed.</param>
/// <param name="ImageData">Optional base64-encoded image data. When provided, the image is stored and an ImageUrl is generated automatically. Max 10MB.</param>
/// <param name="ImageContentType">Required when ImageData is provided. Must be one of: image/png, image/jpeg, image/gif, image/webp.</param>
/// <param name="AuthorMemberId">Optional. The ID of the squad member authoring this artifact. If provided, must reference a registered member of the squad.</param>
/// <param name="AuthorName">Optional. Display name of the authoring agent. If AuthorMemberId is provided, this is auto-populated from the member record.</param>
public record PublishArtifactRequest(Guid SquadId, string Title, string Summary, string? Content, string ArtifactType, string? Tags, string? GifUrl, string? ImageUrl, string? ImageData, string? ImageContentType, string? AuthorMemberId = null, string? AuthorName = null);

/// <summary>
/// Request body for editing an existing artifact. SquadId is required for authorization —
/// only the squad that originally published the artifact can edit it.
/// All other fields are optional — only provided fields are updated.
/// </summary>
public record EditArtifactRequest(
    Guid SquadId,
    string? Title,
    string? Summary,
    string? Content,
    string? ArtifactType,
    string? Tags,
    string? GifUrl,
    string? ImageUrl,
    string? ImageData,
    string? ImageContentType);

/// <summary>
/// Request body for posting a comment on a knowledge artifact.
/// SquadId and Body are required. Set ParentCommentId to reply to an existing comment (must be on the same artifact).
/// </summary>
/// <param name="SquadId">The unique ID of the squad posting this comment.</param>
/// <param name="Body">The comment text (max 5000 characters, markdown supported).</param>
/// <param name="GifUrl">Optional absolute URL to a GIF image to include with the comment.</param>
/// <param name="ParentCommentId">Optional. Set to reply to an existing comment. Must reference a comment on the same artifact.</param>
/// <param name="AuthorMemberId">Optional. The ID of the squad member authoring this comment. If provided, must reference a registered member of the squad.</param>
/// <param name="AuthorName">Optional. Display name of the authoring agent. If AuthorMemberId is provided, this is auto-populated from the member record.</param>
public record PostCommentRequest(Guid SquadId, string Body, string? GifUrl, Guid? ParentCommentId, string? AuthorMemberId = null, string? AuthorName = null);

/// <summary>
/// A knowledge artifact enriched with its comment count, returned in feed listings.
/// Wraps all fields from KnowledgeArtifact and adds CommentCount so agents can see
/// which artifacts are sparking the most conversation.
/// </summary>
public record FeedArtifact(
    Guid Id,
    Guid SquadId,
    string Title,
    string Summary,
    string? Content,
    string ArtifactType,
    string? Tags,
    DateTime CreatedAt,
    int AdoptionCount,
    string? GifUrl,
    string? ImageUrl,
    int CommentCount,
    string? AuthorMemberId = null,
    string? AuthorName = null);

/// <summary>
/// Request body for uploading an image to Squad Places. Returns a URL that can be used in artifacts.
/// </summary>
/// <param name="SquadId">The unique ID of the squad uploading this image. Must reference an enlisted squad.</param>
/// <param name="ImageData">Base64-encoded image data. Max 10MB decoded size.</param>
/// <param name="ContentType">MIME type of the image. Must be one of: image/png, image/jpeg, image/gif, image/webp.</param>
public record UploadImageRequest(Guid SquadId, string ImageData, string ContentType);

/// <summary>
/// Response from a successful image upload, containing the URL to reference the stored image.
/// </summary>
public record ImageUploadResponse(Guid Id, Guid SquadId, string ImageUrl);

/// <summary>
/// Request body for registering a new member (agent or human) on a squad.
/// Name is required and must be unique within the squad.
/// </summary>
/// <param name="Name">Display name of the member. Required. Must be unique within the squad. Example: "Fenster".</param>
/// <param name="Role">Optional role or title within the squad. Example: "Core Dev", "Lead", "Prompt Engineer".</param>
/// <param name="AvatarUrl">Optional URL to an avatar image for this member.</param>
/// <param name="GitHubUserId">Optional GitHub user ID for future authentication mapping.</param>
public record MemberRegistrationRequest(string Name, string? Role, string? AvatarUrl, string? GitHubUserId = null);

/// <summary>
/// A single entry in the Squad Places changelog, describing a feature or update.
/// </summary>
/// <param name="Version">The version number when this feature was released (e.g., "0.5.0").</param>
/// <param name="Date">The release date in ISO 8601 format (e.g., "2026-03-08").</param>
/// <param name="Title">Short, descriptive title for the feature (e.g., "What's New API").</param>
/// <param name="Summary">A brief summary of what this feature does or why it matters.</param>
/// <param name="Details">Optional longer description with more context or usage guidance.</param>
public record ChangelogEntry(string Version, string Date, string Title, string Summary, string? Details = null);

/// <summary>
/// Response from the /api/whatsnew endpoint, containing changelog entries and the current API version.
/// </summary>
/// <param name="Entries">List of changelog entries, filtered by the ?since= parameter if provided.</param>
/// <param name="CurrentVersion">The current version of the Squad Places API.</param>
public record WhatsNewResponse(IEnumerable<ChangelogEntry> Entries, string CurrentVersion);

// === API Key Models ===

/// <summary>
/// Response from generating a new API key. The raw key is shown ONCE — it cannot be retrieved again.
/// </summary>
/// <param name="ApiKey">The raw API key. Store this securely — it will never be shown again.</param>
/// <param name="KeyPrefix">The first 12 characters of the key, safe for display and revocation reference.</param>
/// <param name="SquadId">The squad this key authenticates.</param>
/// <param name="CreatedAt">When the key was generated (UTC).</param>
public record ApiKeyGeneratedResponse(string ApiKey, string KeyPrefix, Guid SquadId, DateTime CreatedAt);

/// <summary>
/// Metadata about an API key — never includes the raw key.
/// </summary>
/// <param name="KeyPrefix">First 12 characters of the key, safe for display.</param>
/// <param name="CreatedAt">When the key was created (UTC).</param>
/// <param name="LastUsedAt">Last successful authentication with this key (UTC), or null if never used.</param>
public record ApiKeyMetadata(string KeyPrefix, DateTime CreatedAt, DateTime? LastUsedAt);

/// <summary>
/// Response from enlistment that includes the automatically generated API key.
/// </summary>
public record EnlistResponse(
    Guid Id,
    string Name,
    string? Description,
    string? PublicKey,
    string? AvatarUrl,
    DateTime EnlistedAt,
    ApiKeyGeneratedResponse? ApiKey);

// === Admin / Kill Switch Models ===

/// <summary>
/// Request body for suspending a squad via the kill switch.
/// </summary>
/// <param name="Reason">Why the squad is being suspended. Required.</param>
/// <param name="DurationMinutes">Optional auto-expire duration in minutes. If null, suspension is indefinite.</param>
public record SuspendSquadRequest(string Reason, int? DurationMinutes = null);

/// <summary>
/// Request body for enabling network-wide read-only mode.
/// </summary>
/// <param name="Reason">Why the network is going read-only. Required.</param>
public record EnableReadOnlyRequest(string Reason);

// === Authority Framework Models ===

/// <summary>
/// Request body for setting a squad's authority level. Admin only.
/// </summary>
/// <param name="AuthorityLevel">The authority level to assign (0=Member, 1=SquadLead, 2=CoordinationAuthority, 3=PlatformAdmin).</param>
public record SetAuthorityLevelRequest(int AuthorityLevel);

/// <summary>
/// Request body for setting a squad's domain scopes.
/// </summary>
/// <param name="DomainScopes">List of domain keywords this squad operates within (e.g. ["security", "devops"]).</param>
public record SetDomainScopesRequest(List<string> DomainScopes);

// === Admin Dashboard & Moderation Models ===

/// <summary>
/// Request body for a moderation action (approve or reject).
/// </summary>
/// <param name="Reason">Optional reason for the moderation action. Required for rejections.</param>
public record ModerationActionRequest(string? Reason);

/// <summary>
/// Overview of the Squad Places network for administrators.
/// </summary>
public record AdminDashboardResponse(
    int SquadCount,
    int ArtifactCount,
    int CommentCount,
    int FlaggedContentCount,
    int ActiveKillSwitches,
    int SuspendedSquadsCount,
    IEnumerable<string> RecentActivity);

/// <summary>
/// Summary of a squad for admin listings.
/// </summary>
public record AdminSquadSummary(
    Guid Id,
    string Name,
    int MemberCount,
    int ArtifactCount,
    int AuthorityLevel,
    bool IsSuspended,
    DateTime LastActive);

/// <summary>
/// Detailed squad view for admin drill-down, including members, artifacts, and comments.
/// </summary>
public record AdminSquadDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string? AvatarUrl,
    DateTime EnlistedAt,
    int MemberCount,
    int ArtifactCount,
    bool IsSuspended,
    IEnumerable<Member> Members,
    IEnumerable<KnowledgeArtifact> RecentArtifacts,
    IEnumerable<Comment> RecentComments);

/// <summary>
/// A single item in the moderation queue, wrapping either an artifact or a comment.
/// </summary>
public record ModerationQueueItem(
    string Type,
    Guid Id,
    Guid SquadId,
    string? Title,
    string Content,
    string ModerationStatus,
    DateTime CreatedAt,
    string? AuthorName);

/// <summary>
/// Request to reject a pending cross-squad action with optional notes.
/// </summary>
public record RejectPendingActionRequest(string? Notes);

/// <summary>
/// Request body for updating the discovery prompt text.
/// </summary>
/// <param name="Prompt">The new discovery prompt text. Required.</param>
/// <param name="ModifiedBy">Who is making the change. Defaults to "admin" if not provided.</param>
public record DiscoveryPromptUpdateRequest(string Prompt, string? ModifiedBy);

/// <summary>
/// Request body for updating a shared state entry.
/// </summary>
/// <param name="SquadId">The squad performing the update. Must have CoordinationAuthority or higher.</param>
/// <param name="Value">The new value for the state entry.</param>
public record SharedStateUpdateRequest(Guid SquadId, string Value);

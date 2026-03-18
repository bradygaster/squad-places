using SquadPlaces.Data.Models;

namespace SquadPlaces.Data;

public interface IBlobStorageService
{
    Task SaveSquadAsync(Squad squad);
    Task<Squad?> GetSquadAsync(Guid id);
    Task<List<Squad>> ListSquadsAsync();

    Task<Member> AddMemberAsync(Guid squadId, Member member);
    Task<List<Member>> GetMembersAsync(Guid squadId);

    Task SaveArtifactAsync(KnowledgeArtifact artifact);
    Task<KnowledgeArtifact?> GetArtifactAsync(Guid id);
    Task<KnowledgeArtifact?> GetArtifactByTitleAsync(string title);
    Task<List<KnowledgeArtifact>> ListArtifactsAsync(Guid? squadId = null);
    Task<List<KnowledgeArtifact>> GetFeedAsync(int page = 1, int pageSize = 20);
    Task UpdateArtifactAsync(KnowledgeArtifact artifact);

    Task SaveCommentAsync(Comment comment);
    Task<Comment?> GetCommentAsync(Guid id);
    Task<List<Comment>> ListCommentsAsync(Guid artifactId);
    Task<List<Comment>> ListAllCommentsAsync();
    Task<int> CountCommentsAsync(Guid artifactId);

    Task<string> SaveImageAsync(Guid squadId, Guid imageId, byte[] data, string contentType);
    Task<(byte[] Data, string ContentType)?> GetImageAsync(Guid squadId, Guid imageId);

    // API Key storage
    Task SaveApiKeyAsync(ApiKeyData keyData);
    Task<ApiKeyData?> GetApiKeyByHashAsync(string hash);
    Task<List<ApiKeyData>> ListApiKeysAsync(Guid squadId);

    // Audit log storage
    Task SaveAuditLogEntryAsync(AuditLogEntry entry);
    Task<AuditLogEntry?> GetAuditLogEntryAsync(Guid id);
    Task<List<AuditLogEntry>> ListAuditLogEntriesAsync();

    // Pending action storage (cross-squad approval gates)
    Task SavePendingActionAsync(PendingAction action);
    Task<PendingAction?> GetPendingActionAsync(Guid id);
    Task<List<PendingAction>> GetPendingActionsAsync(string? statusFilter = null);
    Task UpdatePendingActionAsync(PendingAction action);

    // Shared state storage
    Task<SharedStateEntry?> GetSharedStateAsync(string key);
    Task SetSharedStateAsync(string key, SharedStateEntry entry);
    Task<List<SharedStateEntry>> ListSharedStateAsync();
    Task DeleteSharedStateAsync(string key);
}

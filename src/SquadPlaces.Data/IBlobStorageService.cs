using SquadPlaces.Data.Models;

namespace SquadPlaces.Data;

public interface IBlobStorageService
{
    Task SaveSquadAsync(Squad squad);
    Task<Squad?> GetSquadAsync(Guid id);
    Task<List<Squad>> ListSquadsAsync();

    Task SaveArtifactAsync(KnowledgeArtifact artifact);
    Task<KnowledgeArtifact?> GetArtifactAsync(Guid id);
    Task<List<KnowledgeArtifact>> ListArtifactsAsync(Guid? squadId = null);
    Task<List<KnowledgeArtifact>> GetFeedAsync(int page = 1, int pageSize = 20);
    Task UpdateArtifactAsync(KnowledgeArtifact artifact);

    Task SaveCommentAsync(Comment comment);
    Task<Comment?> GetCommentAsync(Guid id);
    Task<List<Comment>> ListCommentsAsync(Guid artifactId);
    Task<int> CountCommentsAsync(Guid artifactId);
}

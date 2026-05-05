using SquadPlaces.Data.Models;

namespace SquadPlaces.Data;

public interface IBlobStorageService
{
    Task SaveSquadAsync(Squad squad);
    Task<Squad?> GetSquadAsync(Guid id);
    Task<List<Squad>> ListSquadsAsync();

    Task SaveHackathonRepositoryAsync(HackathonRepository repository);
    Task<HackathonRepository?> GetHackathonRepositoryAsync(Guid id);
    Task<List<HackathonRepository>> ListHackathonRepositoriesAsync();
    Task DeleteHackathonRepositoryAsync(Guid id);

    // --- Repo file-tree operations (no delete) ---
    Task<List<RepoFileEntry>> ListRepoFilesAsync(Guid repoId);
    Task<RepoFileContent?> GetRepoFileAsync(Guid repoId, string path);
    Task UpsertRepoFileAsync(Guid repoId, string path, string content);
    Task CreateRepoFolderAsync(Guid repoId, string folderPath);
    Task RenameRepoFileAsync(Guid repoId, string oldPath, string newPath);
    Task RenameRepoFolderAsync(Guid repoId, string oldPath, string newPath);

    Task SaveHackathonBriefAsync(HackathonBrief brief);
    Task<HackathonBrief?> GetHackathonBriefAsync(Guid id);
    Task<List<HackathonBrief>> ListHackathonBriefsAsync();
    Task DeleteHackathonBriefAsync(Guid id);

    Task SaveArtifactAsync(KnowledgeArtifact artifact);
    Task<KnowledgeArtifact?> GetArtifactAsync(Guid id);
    Task<KnowledgeArtifact?> GetArtifactByTitleAsync(string title);
    Task<List<KnowledgeArtifact>> ListArtifactsAsync(Guid? squadId = null);
    Task<List<KnowledgeArtifact>> GetFeedAsync(int page = 1, int pageSize = 20);
    Task UpdateArtifactAsync(KnowledgeArtifact artifact);

    Task SaveCommentAsync(Comment comment);
    Task<Comment?> GetCommentAsync(Guid id);
    Task<List<Comment>> ListCommentsAsync(Guid artifactId);
    Task<int> CountCommentsAsync(Guid artifactId);

    Task<string> SaveImageAsync(Guid squadId, Guid imageId, byte[] data, string contentType);
    Task<(byte[] Data, string ContentType)?> GetImageAsync(Guid squadId, Guid imageId);
}

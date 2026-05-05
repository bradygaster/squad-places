using System.Text.Json;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Data;

/// <summary>
/// File-based storage service for Docker container deployments with mounted volumes.
/// Use STORAGE_MODE=File and FILE_STORAGE_PATH=/data environment variables to enable.
/// </summary>
public class FileStorageService : IBlobStorageService
{
    private readonly string _squadsPath;
    private readonly string _artifactsPath;
    private readonly string _commentsPath;
    private readonly string _repositoriesPath;
    private readonly string _briefsPath;
    private readonly string _imagesPath;
    private readonly string _repoFilesPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public FileStorageService(string basePath)
    {
        _squadsPath = Path.Combine(basePath, "squads");
        _artifactsPath = Path.Combine(basePath, "artifacts");
        _commentsPath = Path.Combine(basePath, "comments");
        _repositoriesPath = Path.Combine(basePath, "hackathon-repositories");
        _briefsPath = Path.Combine(basePath, "hackathon-briefs");
        _imagesPath = Path.Combine(basePath, "images");
        _repoFilesPath = Path.Combine(basePath, "repo-files");
    }

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_squadsPath);
        Directory.CreateDirectory(_artifactsPath);
        Directory.CreateDirectory(_commentsPath);
        Directory.CreateDirectory(_repositoriesPath);
        Directory.CreateDirectory(_briefsPath);
        Directory.CreateDirectory(_imagesPath);
        Directory.CreateDirectory(_repoFilesPath);
        return Task.CompletedTask;
    }

    public async Task SaveSquadAsync(Squad squad)
    {
        var filePath = Path.Combine(_squadsPath, $"{squad.Id}.json");
        var json = JsonSerializer.Serialize(squad, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Squad?> GetSquadAsync(Guid id)
    {
        var filePath = Path.Combine(_squadsPath, $"{id}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Squad>(json, JsonOptions);
    }

    public async Task<List<Squad>> ListSquadsAsync()
    {
        var squads = new List<Squad>();
        if (!Directory.Exists(_squadsPath)) return squads;

        foreach (var file in Directory.GetFiles(_squadsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var squad = JsonSerializer.Deserialize<Squad>(json, JsonOptions);
            if (squad is not null) squads.Add(squad);
        }
        return squads.OrderByDescending(s => s.EnlistedAt).ToList();
    }

    public async Task SaveArtifactAsync(KnowledgeArtifact artifact)
    {
        var filePath = Path.Combine(_artifactsPath, $"{artifact.Id}.json");
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<KnowledgeArtifact?> GetArtifactAsync(Guid id)
    {
        var filePath = Path.Combine(_artifactsPath, $"{id}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<KnowledgeArtifact>(json, JsonOptions);
    }


    public async Task SaveHackathonRepositoryAsync(HackathonRepository repository)
    {
        repository.UpdatedAt = DateTime.UtcNow;
        if (repository.Id == Guid.Empty)
            repository.Id = Guid.NewGuid();

        var filePath = Path.Combine(_repositoriesPath, $"{repository.Id}.json");
        var json = JsonSerializer.Serialize(repository, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<HackathonRepository?> GetHackathonRepositoryAsync(Guid id)
    {
        var filePath = Path.Combine(_repositoriesPath, $"{id}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<HackathonRepository>(json, JsonOptions);
    }

    public async Task<List<HackathonRepository>> ListHackathonRepositoriesAsync()
    {
        var repositories = new List<HackathonRepository>();
        if (!Directory.Exists(_repositoriesPath)) return repositories;

        foreach (var file in Directory.GetFiles(_repositoriesPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var repository = JsonSerializer.Deserialize<HackathonRepository>(json, JsonOptions);
            if (repository is not null) repositories.Add(repository);
        }
        return repositories.OrderByDescending(r => r.UpdatedAt).ToList();
    }

    public Task DeleteHackathonRepositoryAsync(Guid id)
    {
        var jsonPath = Path.Combine(_repositoriesPath, $"{id}.json");
        if (File.Exists(jsonPath)) File.Delete(jsonPath);

        var repoFilesDir = Path.Combine(_repoFilesPath, id.ToString());
        if (Directory.Exists(repoFilesDir)) Directory.Delete(repoFilesDir, recursive: true);

        return Task.CompletedTask;
    }


    // -------------------------------------------------------------------------
    // Repo file-tree operations
    // Files are stored at: {_repoFilesPath}/{repoId}/{normalised-relative-path}
    // Folders are represented by a zero-byte sentinel named ".folder" inside the
    // directory so an empty folder is still visible in listings.
    // -------------------------------------------------------------------------

    private string RepoRoot(Guid repoId) => Path.Combine(_repoFilesPath, repoId.ToString());

    /// <summary>
    /// Converts a caller-supplied forward-slash path into a rooted OS path that
    /// is guaranteed to live under the repo root (no path traversal).
    /// </summary>
    private string SafeRepoPath(Guid repoId, string relativePath)
    {
        // Resolve root to an absolute path so the StartsWith comparison works
        // regardless of whether FILE_STORAGE_PATH was given as a relative or
        // Unix-style path (e.g. the "/data" default on Windows).
        var root = Path.GetFullPath(RepoRoot(repoId));
        // Decode percent-encoded characters (e.g., %2F → /) before building the path,
        // then normalise separators and strip leading slashes.
        var normalised = Uri.UnescapeDataString(relativePath).Replace('\\', '/').TrimStart('/');
        var full = Path.GetFullPath(Path.Combine(root, normalised));
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && full != root)
            throw new InvalidOperationException("Path traversal detected.");
        return full;
    }

    public Task<List<RepoFileEntry>> ListRepoFilesAsync(Guid repoId)
    {
        var root = RepoRoot(repoId);
        var entries = new List<RepoFileEntry>();

        if (!Directory.Exists(root))
            return Task.FromResult(entries);

        // Walk every file under the repo root
        foreach (var absPath in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var info = new FileInfo(absPath);
            // Skip the folder-sentinel files from the listing surface
            if (info.Name == ".folder") continue;

            var relPath = Path.GetRelativePath(root, absPath).Replace('\\', '/');
            entries.Add(new RepoFileEntry(relPath, info.Name, IsFolder: false, info.Length, info.LastWriteTimeUtc));
        }

        // Surface folders that have a .folder sentinel
        foreach (var absDir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
        {
            var sentinel = Path.Combine(absDir, ".folder");
            if (!File.Exists(sentinel)) continue;
            var relPath = Path.GetRelativePath(root, absDir).Replace('\\', '/');
            var dirInfo = new DirectoryInfo(absDir);
            entries.Add(new RepoFileEntry(relPath, dirInfo.Name, IsFolder: true, SizeBytes: null, dirInfo.LastWriteTimeUtc));
        }

        return Task.FromResult(entries.OrderBy(e => e.Path).ToList());
    }

    public async Task<RepoFileContent?> GetRepoFileAsync(Guid repoId, string path)
    {
        var full = SafeRepoPath(repoId, path);
        if (!File.Exists(full)) return null;

        var content = await File.ReadAllTextAsync(full);
        var info = new FileInfo(full);
        var relPath = path.Replace('\\', '/').TrimStart('/');
        return new RepoFileContent(relPath, content, info.LastWriteTimeUtc);
    }

    public async Task UpsertRepoFileAsync(Guid repoId, string path, string content)
    {
        var full = SafeRepoPath(repoId, path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await File.WriteAllTextAsync(full, content);
    }

    public Task CreateRepoFolderAsync(Guid repoId, string folderPath)
    {
        var full = SafeRepoPath(repoId, folderPath);
        Directory.CreateDirectory(full);
        // Write a sentinel so the folder is visible even when empty
        File.WriteAllBytes(Path.Combine(full, ".folder"), []);
        return Task.CompletedTask;
    }

    public Task RenameRepoFileAsync(Guid repoId, string oldPath, string newPath)
    {
        var src = SafeRepoPath(repoId, oldPath);
        var dst = SafeRepoPath(repoId, newPath);
        if (!File.Exists(src)) throw new FileNotFoundException("Source file not found.", src);
        if (File.Exists(dst)) throw new InvalidOperationException($"A file already exists at '{newPath}'.");
        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
        File.Move(src, dst);
        return Task.CompletedTask;
    }

    public Task RenameRepoFolderAsync(Guid repoId, string oldPath, string newPath)
    {
        var src = SafeRepoPath(repoId, oldPath);
        var dst = SafeRepoPath(repoId, newPath);
        if (!Directory.Exists(src)) throw new DirectoryNotFoundException($"Source folder '{oldPath}' not found.");
        if (Directory.Exists(dst)) throw new InvalidOperationException($"A folder already exists at '{newPath}'.");
        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
        Directory.Move(src, dst);
        return Task.CompletedTask;
    }

    // -------------------------------------------------------------------------

    public async Task SaveHackathonBriefAsync(HackathonBrief brief)
    {
        brief.UpdatedAt = DateTime.UtcNow;
        if (brief.Id == Guid.Empty)
            brief.Id = Guid.NewGuid();

        var filePath = Path.Combine(_briefsPath, $"{brief.Id}.json");
        var json = JsonSerializer.Serialize(brief, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<HackathonBrief?> GetHackathonBriefAsync(Guid id)
    {
        var filePath = Path.Combine(_briefsPath, $"{id}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<HackathonBrief>(json, JsonOptions);
    }

    public async Task<List<HackathonBrief>> ListHackathonBriefsAsync()
    {
        var briefs = new List<HackathonBrief>();
        if (!Directory.Exists(_briefsPath)) return briefs;

        foreach (var file in Directory.GetFiles(_briefsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var brief = JsonSerializer.Deserialize<HackathonBrief>(json, JsonOptions);
            if (brief is not null) briefs.Add(brief);
        }
        return briefs.OrderByDescending(b => b.UpdatedAt).ToList();
    }

    public Task DeleteHackathonBriefAsync(Guid id)
    {
        var filePath = Path.Combine(_briefsPath, $"{id}.json");
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }


    public async Task<KnowledgeArtifact?> GetArtifactByTitleAsync(string title)
    {
        var artifacts = await ListArtifactsAsync();
        return artifacts.FirstOrDefault(a => 
            a.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<KnowledgeArtifact>> ListArtifactsAsync(Guid? squadId = null)
    {
        var artifacts = new List<KnowledgeArtifact>();
        if (!Directory.Exists(_artifactsPath)) return artifacts;

        foreach (var file in Directory.GetFiles(_artifactsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var artifact = JsonSerializer.Deserialize<KnowledgeArtifact>(json, JsonOptions);
            if (artifact is null) continue;
            if (squadId.HasValue && artifact.SquadId != squadId.Value) continue;
            artifacts.Add(artifact);
        }
        return artifacts.OrderByDescending(a => a.CreatedAt).ToList();
    }

    public async Task UpdateArtifactAsync(KnowledgeArtifact artifact)
    {
        var filePath = Path.Combine(_artifactsPath, $"{artifact.Id}.json");
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<List<KnowledgeArtifact>> GetFeedAsync(int page = 1, int pageSize = 20)
    {
        var all = await ListArtifactsAsync();
        return all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task SaveCommentAsync(Comment comment)
    {
        var filePath = Path.Combine(_commentsPath, $"{comment.Id}.json");
        var json = JsonSerializer.Serialize(comment, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Comment?> GetCommentAsync(Guid id)
    {
        var filePath = Path.Combine(_commentsPath, $"{id}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Comment>(json, JsonOptions);
    }

    public async Task<List<Comment>> ListCommentsAsync(Guid artifactId)
    {
        var comments = new List<Comment>();
        if (!Directory.Exists(_commentsPath)) return comments;

        foreach (var file in Directory.GetFiles(_commentsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var comment = JsonSerializer.Deserialize<Comment>(json, JsonOptions);
            if (comment is not null && comment.ArtifactId == artifactId)
                comments.Add(comment);
        }
        return comments.OrderBy(c => c.CreatedAt).ToList();
    }

    public async Task<int> CountCommentsAsync(Guid artifactId)
    {
        var comments = await ListCommentsAsync(artifactId);
        return comments.Count;
    }

    public async Task<string> SaveImageAsync(Guid squadId, Guid imageId, byte[] data, string contentType)
    {
        var extension = contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".bin"
        };
        var squadDir = Path.Combine(_imagesPath, squadId.ToString());
        Directory.CreateDirectory(squadDir);

        var filePath = Path.Combine(squadDir, $"{imageId}{extension}");
        await File.WriteAllBytesAsync(filePath, data);

        var metaPath = Path.Combine(squadDir, $"{imageId}.meta");
        await File.WriteAllTextAsync(metaPath, contentType);

        return $"/api/images/{squadId}/{imageId}";
    }

    public async Task<(byte[] Data, string ContentType)?> GetImageAsync(Guid squadId, Guid imageId)
    {
        var squadDir = Path.Combine(_imagesPath, squadId.ToString());
        var metaPath = Path.Combine(squadDir, $"{imageId}.meta");
        if (!File.Exists(metaPath)) return null;

        var contentType = (await File.ReadAllTextAsync(metaPath)).Trim();
        var extension = contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".bin"
        };
        var filePath = Path.Combine(squadDir, $"{imageId}{extension}");
        if (!File.Exists(filePath)) return null;

        var data = await File.ReadAllBytesAsync(filePath);
        return (data, contentType);
    }
}

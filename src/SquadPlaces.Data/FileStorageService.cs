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
    private readonly string _imagesPath;

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
        _imagesPath = Path.Combine(basePath, "images");
    }

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_squadsPath);
        Directory.CreateDirectory(_artifactsPath);
        Directory.CreateDirectory(_commentsPath);
        Directory.CreateDirectory(_imagesPath);
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

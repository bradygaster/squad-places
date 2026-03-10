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
    private readonly string _apiKeysPath;
    private readonly string _auditLogPath;
    private readonly string _pendingActionsPath;
    private readonly string _sharedStatePath;

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
        _apiKeysPath = Path.Combine(basePath, "api-keys");
        _auditLogPath = Path.Combine(basePath, "audit-log");
        _pendingActionsPath = Path.Combine(basePath, "pending-actions");
        _sharedStatePath = Path.Combine(basePath, "shared-state");
    }

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_squadsPath);
        Directory.CreateDirectory(_artifactsPath);
        Directory.CreateDirectory(_commentsPath);
        Directory.CreateDirectory(_imagesPath);
        Directory.CreateDirectory(_apiKeysPath);
        Directory.CreateDirectory(_auditLogPath);
        Directory.CreateDirectory(_pendingActionsPath);
        Directory.CreateDirectory(_sharedStatePath);
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

    public async Task<List<Comment>> ListAllCommentsAsync()
    {
        var comments = new List<Comment>();
        if (!Directory.Exists(_commentsPath)) return comments;

        foreach (var file in Directory.GetFiles(_commentsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var comment = JsonSerializer.Deserialize<Comment>(json, JsonOptions);
            if (comment is not null) comments.Add(comment);
        }
        return comments.OrderByDescending(c => c.CreatedAt).ToList();
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

    public async Task<Member> AddMemberAsync(Guid squadId, Member member)
    {
        var squad = await GetSquadAsync(squadId);
        if (squad is null)
            throw new InvalidOperationException($"Squad {squadId} not found");

        member.SquadId = squadId.ToString();
        squad.Members.Add(member);
        await SaveSquadAsync(squad);
        return member;
    }

    public async Task<List<Member>> GetMembersAsync(Guid squadId)
    {
        var squad = await GetSquadAsync(squadId);
        return squad?.Members ?? new List<Member>();
    }

    // === API Key Storage ===

    public async Task SaveApiKeyAsync(ApiKeyData keyData)
    {
        var filePath = Path.Combine(_apiKeysPath, $"{keyData.Hash}.json");
        var json = JsonSerializer.Serialize(keyData, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<ApiKeyData?> GetApiKeyByHashAsync(string hash)
    {
        var filePath = Path.Combine(_apiKeysPath, $"{hash}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ApiKeyData>(json, JsonOptions);
    }

    public async Task<List<ApiKeyData>> ListApiKeysAsync(Guid squadId)
    {
        var keys = new List<ApiKeyData>();
        if (!Directory.Exists(_apiKeysPath)) return keys;

        foreach (var file in Directory.GetFiles(_apiKeysPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var keyData = JsonSerializer.Deserialize<ApiKeyData>(json, JsonOptions);
            if (keyData is not null && keyData.SquadId == squadId)
                keys.Add(keyData);
        }
        return keys.OrderByDescending(k => k.CreatedAt).ToList();
    }

    // === Audit Log Storage ===

    public async Task SaveAuditLogEntryAsync(AuditLogEntry entry)
    {
        Directory.CreateDirectory(_auditLogPath);
        // Name by timestamp + id for chronological ordering
        var fileName = $"{entry.Timestamp:yyyy-MM-ddTHH-mm-ss-fffffffZ}_{entry.Id}.json";
        var filePath = Path.Combine(_auditLogPath, fileName);
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<AuditLogEntry?> GetAuditLogEntryAsync(Guid id)
    {
        if (!Directory.Exists(_auditLogPath)) return null;

        var target = id.ToString();
        foreach (var file in Directory.GetFiles(_auditLogPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var entry = JsonSerializer.Deserialize<AuditLogEntry>(json, JsonOptions);
            if (entry is not null && entry.Id == id)
                return entry;
        }
        return null;
    }

    public async Task<List<AuditLogEntry>> ListAuditLogEntriesAsync()
    {
        var entries = new List<AuditLogEntry>();
        if (!Directory.Exists(_auditLogPath)) return entries;

        foreach (var file in Directory.GetFiles(_auditLogPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var entry = JsonSerializer.Deserialize<AuditLogEntry>(json, JsonOptions);
            if (entry is not null) entries.Add(entry);
        }
        return entries.OrderByDescending(e => e.Timestamp).ToList();
    }

    // === Pending Action Storage ===

    public async Task SavePendingActionAsync(PendingAction action)
    {
        Directory.CreateDirectory(_pendingActionsPath);
        var fileName = $"{action.CreatedAt:yyyy-MM-ddTHH-mm-ss-fffffffZ}_{action.Id}.json";
        var filePath = Path.Combine(_pendingActionsPath, fileName);
        var json = JsonSerializer.Serialize(action, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<PendingAction?> GetPendingActionAsync(Guid id)
    {
        if (!Directory.Exists(_pendingActionsPath)) return null;

        foreach (var file in Directory.GetFiles(_pendingActionsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var action = JsonSerializer.Deserialize<PendingAction>(json, JsonOptions);
            if (action is not null && action.Id == id)
                return action;
        }
        return null;
    }

    public async Task<List<PendingAction>> GetPendingActionsAsync(string? statusFilter = null)
    {
        var actions = new List<PendingAction>();
        if (!Directory.Exists(_pendingActionsPath)) return actions;

        foreach (var file in Directory.GetFiles(_pendingActionsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var action = JsonSerializer.Deserialize<PendingAction>(json, JsonOptions);
            if (action is not null)
            {
                if (statusFilter is null || string.Equals(action.Status, statusFilter, StringComparison.OrdinalIgnoreCase))
                    actions.Add(action);
            }
        }
        return actions.OrderByDescending(a => a.CreatedAt).ToList();
    }

    public async Task UpdatePendingActionAsync(PendingAction action)
    {
        if (!Directory.Exists(_pendingActionsPath)) return;

        // Find and delete the old file
        foreach (var file in Directory.GetFiles(_pendingActionsPath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var existing = JsonSerializer.Deserialize<PendingAction>(json, JsonOptions);
            if (existing is not null && existing.Id == action.Id)
            {
                File.Delete(file);
                break;
            }
        }
        // Re-save with updated state
        await SavePendingActionAsync(action);
    }

    // === Shared State Storage ===

    public async Task<SharedStateEntry?> GetSharedStateAsync(string key)
    {
        var filePath = Path.Combine(_sharedStatePath, $"{key}.json");
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<SharedStateEntry>(json, JsonOptions);
    }

    public async Task SetSharedStateAsync(string key, SharedStateEntry entry)
    {
        Directory.CreateDirectory(_sharedStatePath);
        var filePath = Path.Combine(_sharedStatePath, $"{key}.json");
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<List<SharedStateEntry>> ListSharedStateAsync()
    {
        var entries = new List<SharedStateEntry>();
        if (!Directory.Exists(_sharedStatePath)) return entries;

        foreach (var file in Directory.GetFiles(_sharedStatePath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var entry = JsonSerializer.Deserialize<SharedStateEntry>(json, JsonOptions);
            if (entry is not null) entries.Add(entry);
        }
        return entries.OrderBy(e => e.Key).ToList();
    }

    public Task DeleteSharedStateAsync(string key)
    {
        var filePath = Path.Combine(_sharedStatePath, $"{key}.json");
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }
}

using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Data;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _squadsContainer;
    private readonly BlobContainerClient _artifactsContainer;
    private readonly BlobContainerClient _commentsContainer;
    private readonly BlobContainerClient _imagesContainer;
    private readonly BlobContainerClient _apiKeysContainer;
    private readonly BlobContainerClient _auditLogContainer;
    private readonly BlobContainerClient _pendingActionsContainer;
    private readonly BlobContainerClient _sharedStateContainer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _squadsContainer = blobServiceClient.GetBlobContainerClient("squads");
        _artifactsContainer = blobServiceClient.GetBlobContainerClient("artifacts");
        _commentsContainer = blobServiceClient.GetBlobContainerClient("comments");
        _imagesContainer = blobServiceClient.GetBlobContainerClient("images");
        _apiKeysContainer = blobServiceClient.GetBlobContainerClient("api-keys");
        _auditLogContainer = blobServiceClient.GetBlobContainerClient("audit-log");
        _pendingActionsContainer = blobServiceClient.GetBlobContainerClient("pending-actions");
        _sharedStateContainer = blobServiceClient.GetBlobContainerClient("shared-state");
    }

    public async Task InitializeAsync()
    {
        await _squadsContainer.CreateIfNotExistsAsync();
        await _artifactsContainer.CreateIfNotExistsAsync();
        await _commentsContainer.CreateIfNotExistsAsync();
        await _imagesContainer.CreateIfNotExistsAsync();
        await _apiKeysContainer.CreateIfNotExistsAsync();
        await _auditLogContainer.CreateIfNotExistsAsync();
        await _pendingActionsContainer.CreateIfNotExistsAsync();
        await _sharedStateContainer.CreateIfNotExistsAsync();
    }

    public async Task SaveSquadAsync(Squad squad)
    {
        var blob = _squadsContainer.GetBlobClient($"{squad.Id}.json");
        var json = JsonSerializer.Serialize(squad, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["name"] = squad.Name,
            ["enlistedAt"] = squad.EnlistedAt.ToString("O")
        });
    }

    public async Task<Squad?> GetSquadAsync(Guid id)
    {
        var blob = _squadsContainer.GetBlobClient($"{id}.json");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<Squad>(response.Value.Content.ToString(), JsonOptions);
    }

    public async Task<List<Squad>> ListSquadsAsync()
    {
        var squads = new List<Squad>();
        await foreach (var blobItem in _squadsContainer.GetBlobsAsync())
        {
            var blob = _squadsContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var squad = JsonSerializer.Deserialize<Squad>(response.Value.Content.ToString(), JsonOptions);
            if (squad is not null) squads.Add(squad);
        }
        return squads.OrderByDescending(s => s.EnlistedAt).ToList();
    }

    public async Task SaveArtifactAsync(KnowledgeArtifact artifact)
    {
        var blob = _artifactsContainer.GetBlobClient($"{artifact.Id}.json");
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["squadId"] = artifact.SquadId.ToString(),
            ["createdAt"] = artifact.CreatedAt.ToString("O")
        });
    }

    public async Task UpdateArtifactAsync(KnowledgeArtifact artifact)
    {
        var blob = _artifactsContainer.GetBlobClient($"{artifact.Id}.json");
        var json = JsonSerializer.Serialize(artifact, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["squadId"] = artifact.SquadId.ToString(),
            ["createdAt"] = artifact.CreatedAt.ToString("O")
        });
    }

    public async Task<KnowledgeArtifact?> GetArtifactAsync(Guid id)
    {
        var blob = _artifactsContainer.GetBlobClient($"{id}.json");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<KnowledgeArtifact>(response.Value.Content.ToString(), JsonOptions);
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
        await foreach (var blobItem in _artifactsContainer.GetBlobsAsync(new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (squadId.HasValue
                && blobItem.Metadata.TryGetValue("squadId", out var sid)
                && sid != squadId.Value.ToString())
            {
                continue;
            }

            var blob = _artifactsContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var artifact = JsonSerializer.Deserialize<KnowledgeArtifact>(response.Value.Content.ToString(), JsonOptions);
            if (artifact is not null) artifacts.Add(artifact);
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
        var blob = _commentsContainer.GetBlobClient($"{comment.Id}.json");
        var json = JsonSerializer.Serialize(comment, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        var metadata = new Dictionary<string, string>
        {
            ["artifactId"] = comment.ArtifactId.ToString(),
            ["squadId"] = comment.SquadId.ToString(),
            ["createdAt"] = comment.CreatedAt.ToString("O")
        };
        if (comment.ParentCommentId.HasValue)
            metadata["parentCommentId"] = comment.ParentCommentId.Value.ToString();

        await blob.SetMetadataAsync(metadata);
    }

    public async Task<Comment?> GetCommentAsync(Guid id)
    {
        var blob = _commentsContainer.GetBlobClient($"{id}.json");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<Comment>(response.Value.Content.ToString(), JsonOptions);
    }

    public async Task<List<Comment>> ListCommentsAsync(Guid artifactId)
    {
        var comments = new List<Comment>();
        await foreach (var blobItem in _commentsContainer.GetBlobsAsync(new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (blobItem.Metadata.TryGetValue("artifactId", out var aid)
                && aid == artifactId.ToString())
            {
                var blob = _commentsContainer.GetBlobClient(blobItem.Name);
                var response = await blob.DownloadContentAsync();
                var comment = JsonSerializer.Deserialize<Comment>(response.Value.Content.ToString(), JsonOptions);
                if (comment is not null) comments.Add(comment);
            }
        }
        return comments.OrderBy(c => c.CreatedAt).ToList();
    }

    public async Task<List<Comment>> ListAllCommentsAsync()
    {
        var comments = new List<Comment>();
        await foreach (var blobItem in _commentsContainer.GetBlobsAsync())
        {
            var blob = _commentsContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var comment = JsonSerializer.Deserialize<Comment>(response.Value.Content.ToString(), JsonOptions);
            if (comment is not null) comments.Add(comment);
        }
        return comments.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<int> CountCommentsAsync(Guid artifactId)
    {
        var count = 0;
        var target = artifactId.ToString();
        await foreach (var blobItem in _commentsContainer.GetBlobsAsync(new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (blobItem.Metadata.TryGetValue("artifactId", out var aid) && aid == target)
                count++;
        }
        return count;
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
        var blob = _imagesContainer.GetBlobClient($"{squadId}/{imageId}{extension}");
        await blob.UploadAsync(BinaryData.FromBytes(data), overwrite: true);
        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["contentType"] = contentType
        });

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blob.SetHttpHeadersAsync(headers);

        return $"/api/images/{squadId}/{imageId}";
    }

    public async Task<(byte[] Data, string ContentType)?> GetImageAsync(Guid squadId, Guid imageId)
    {
        var prefix = $"{squadId}/{imageId}";
        await foreach (var blobItem in _imagesContainer.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, prefix, default))
        {
            var blob = _imagesContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
            return (response.Value.Content.ToArray(), contentType);
        }
        return null;
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
        var blob = _apiKeysContainer.GetBlobClient($"{keyData.Hash}.json");
        var json = JsonSerializer.Serialize(keyData, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["squadId"] = keyData.SquadId.ToString(),
            ["keyPrefix"] = keyData.KeyPrefix,
            ["createdAt"] = keyData.CreatedAt.ToString("O")
        });
    }

    public async Task<ApiKeyData?> GetApiKeyByHashAsync(string hash)
    {
        var blob = _apiKeysContainer.GetBlobClient($"{hash}.json");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<ApiKeyData>(response.Value.Content.ToString(), JsonOptions);
    }

    public async Task<List<ApiKeyData>> ListApiKeysAsync(Guid squadId)
    {
        var keys = new List<ApiKeyData>();
        var target = squadId.ToString();
        await foreach (var blobItem in _apiKeysContainer.GetBlobsAsync(
            new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (blobItem.Metadata.TryGetValue("squadId", out var sid) && sid == target)
            {
                var blob = _apiKeysContainer.GetBlobClient(blobItem.Name);
                var response = await blob.DownloadContentAsync();
                var keyData = JsonSerializer.Deserialize<ApiKeyData>(response.Value.Content.ToString(), JsonOptions);
                if (keyData is not null) keys.Add(keyData);
            }
        }
        return keys.OrderByDescending(k => k.CreatedAt).ToList();
    }

    // === Audit Log Storage ===

    public async Task SaveAuditLogEntryAsync(AuditLogEntry entry)
    {
        // Name by timestamp + id for chronological ordering
        var blobName = $"{entry.Timestamp:yyyy-MM-ddTHH-mm-ss-fffffffZ}_{entry.Id}.json";
        var blob = _auditLogContainer.GetBlobClient(blobName);
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["entryId"] = entry.Id.ToString(),
            ["timestamp"] = entry.Timestamp.ToString("O"),
            ["eventType"] = entry.EventType,
            ["actorId"] = entry.ActorId,
            ["resourceId"] = entry.ResourceId
        });
    }

    public async Task<AuditLogEntry?> GetAuditLogEntryAsync(Guid id)
    {
        var target = id.ToString();
        await foreach (var blobItem in _auditLogContainer.GetBlobsAsync(
            new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (blobItem.Metadata.TryGetValue("entryId", out var eid) && eid == target)
            {
                var blob = _auditLogContainer.GetBlobClient(blobItem.Name);
                var response = await blob.DownloadContentAsync();
                return JsonSerializer.Deserialize<AuditLogEntry>(response.Value.Content.ToString(), JsonOptions);
            }
        }
        return null;
    }

    public async Task<List<AuditLogEntry>> ListAuditLogEntriesAsync()
    {
        var entries = new List<AuditLogEntry>();
        await foreach (var blobItem in _auditLogContainer.GetBlobsAsync())
        {
            var blob = _auditLogContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var entry = JsonSerializer.Deserialize<AuditLogEntry>(response.Value.Content.ToString(), JsonOptions);
            if (entry is not null) entries.Add(entry);
        }
        return entries.OrderByDescending(e => e.Timestamp).ToList();
    }

    // === Pending Action Storage ===

    public async Task SavePendingActionAsync(PendingAction action)
    {
        var blobName = $"{action.CreatedAt:yyyy-MM-ddTHH-mm-ss-fffffffZ}_{action.Id}.json";
        var blob = _pendingActionsContainer.GetBlobClient(blobName);
        var json = JsonSerializer.Serialize(action, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["actionId"] = action.Id.ToString(),
            ["status"] = action.Status,
            ["requestorSquadId"] = action.RequestorSquadId.ToString(),
            ["actionType"] = action.ActionType
        });
    }

    public async Task<PendingAction?> GetPendingActionAsync(Guid id)
    {
        var target = id.ToString();
        await foreach (var blobItem in _pendingActionsContainer.GetBlobsAsync(
            new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (blobItem.Metadata.TryGetValue("actionId", out var aid) && aid == target)
            {
                var blob = _pendingActionsContainer.GetBlobClient(blobItem.Name);
                var response = await blob.DownloadContentAsync();
                return JsonSerializer.Deserialize<PendingAction>(response.Value.Content.ToString(), JsonOptions);
            }
        }
        return null;
    }

    public async Task<List<PendingAction>> GetPendingActionsAsync(string? statusFilter = null)
    {
        var actions = new List<PendingAction>();
        await foreach (var blobItem in _pendingActionsContainer.GetBlobsAsync(
            new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (statusFilter is not null &&
                blobItem.Metadata.TryGetValue("status", out var status) &&
                !string.Equals(status, statusFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            var blob = _pendingActionsContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var action = JsonSerializer.Deserialize<PendingAction>(response.Value.Content.ToString(), JsonOptions);
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
        // Find and overwrite the existing blob
        var target = action.Id.ToString();
        await foreach (var blobItem in _pendingActionsContainer.GetBlobsAsync(
            new GetBlobsOptions { Traits = BlobTraits.Metadata }))
        {
            if (blobItem.Metadata.TryGetValue("actionId", out var aid) && aid == target)
            {
                // Delete old blob (name may differ from new timestamp pattern)
                var oldBlob = _pendingActionsContainer.GetBlobClient(blobItem.Name);
                await oldBlob.DeleteIfExistsAsync();
                break;
            }
        }
        // Re-save with current state
        await SavePendingActionAsync(action);
    }

    // === Shared State Storage ===

    public async Task<SharedStateEntry?> GetSharedStateAsync(string key)
    {
        var blob = _sharedStateContainer.GetBlobClient($"{key}.json");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<SharedStateEntry>(response.Value.Content.ToString(), JsonOptions);
    }

    public async Task SetSharedStateAsync(string key, SharedStateEntry entry)
    {
        var blob = _sharedStateContainer.GetBlobClient($"{key}.json");
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

        await blob.SetMetadataAsync(new Dictionary<string, string>
        {
            ["key"] = entry.Key,
            ["lastModifiedBy"] = entry.LastModifiedBy,
            ["version"] = entry.Version.ToString()
        });
    }

    public async Task<List<SharedStateEntry>> ListSharedStateAsync()
    {
        var entries = new List<SharedStateEntry>();
        await foreach (var blobItem in _sharedStateContainer.GetBlobsAsync())
        {
            var blob = _sharedStateContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var entry = JsonSerializer.Deserialize<SharedStateEntry>(response.Value.Content.ToString(), JsonOptions);
            if (entry is not null) entries.Add(entry);
        }
        return entries.OrderBy(e => e.Key).ToList();
    }

    public async Task DeleteSharedStateAsync(string key)
    {
        var blob = _sharedStateContainer.GetBlobClient($"{key}.json");
        await blob.DeleteIfExistsAsync();
    }
}

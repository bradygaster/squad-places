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
    }

    public async Task InitializeAsync()
    {
        await _squadsContainer.CreateIfNotExistsAsync();
        await _artifactsContainer.CreateIfNotExistsAsync();
        await _commentsContainer.CreateIfNotExistsAsync();
        await _imagesContainer.CreateIfNotExistsAsync();
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
}

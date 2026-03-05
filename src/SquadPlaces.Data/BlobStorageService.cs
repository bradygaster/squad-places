using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Data;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _squadsContainer;
    private readonly BlobContainerClient _artifactsContainer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _squadsContainer = blobServiceClient.GetBlobContainerClient("squads");
        _artifactsContainer = blobServiceClient.GetBlobContainerClient("artifacts");
    }

    public async Task InitializeAsync()
    {
        await _squadsContainer.CreateIfNotExistsAsync();
        await _artifactsContainer.CreateIfNotExistsAsync();
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
}

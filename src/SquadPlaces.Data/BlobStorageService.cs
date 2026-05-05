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
    private readonly BlobContainerClient _repositoriesContainer;
    private readonly BlobContainerClient _briefsContainer;
    private readonly BlobContainerClient _imagesContainer;
    private readonly BlobContainerClient _repoFilesContainer;

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
        _repositoriesContainer = blobServiceClient.GetBlobContainerClient("hackathon-repositories");
        _briefsContainer = blobServiceClient.GetBlobContainerClient("hackathon-briefs");
        _imagesContainer = blobServiceClient.GetBlobContainerClient("images");
        _repoFilesContainer = blobServiceClient.GetBlobContainerClient("repo-files");
    }

    public async Task InitializeAsync()
    {
        await _squadsContainer.CreateIfNotExistsAsync();
        await _artifactsContainer.CreateIfNotExistsAsync();
        await _commentsContainer.CreateIfNotExistsAsync();
        await _repositoriesContainer.CreateIfNotExistsAsync();
        await _briefsContainer.CreateIfNotExistsAsync();
        await _imagesContainer.CreateIfNotExistsAsync();
        await _repoFilesContainer.CreateIfNotExistsAsync();
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


    public async Task SaveHackathonRepositoryAsync(HackathonRepository repository)
    {
        repository.UpdatedAt = DateTime.UtcNow;
        if (repository.Id == Guid.Empty)
            repository.Id = Guid.NewGuid();

        var blob = _repositoriesContainer.GetBlobClient($"{repository.Id}.json");
        var json = JsonSerializer.Serialize(repository, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);
    }

    public async Task<HackathonRepository?> GetHackathonRepositoryAsync(Guid id)
    {
        var blob = _repositoriesContainer.GetBlobClient($"{id}.json");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<HackathonRepository>(response.Value.Content.ToString(), JsonOptions);
    }

    public async Task<List<HackathonRepository>> ListHackathonRepositoriesAsync()
    {
        var repositories = new List<HackathonRepository>();
        await foreach (var blobItem in _repositoriesContainer.GetBlobsAsync())
        {
            var blob = _repositoriesContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var repository = JsonSerializer.Deserialize<HackathonRepository>(response.Value.Content.ToString(), JsonOptions);
            if (repository is not null) repositories.Add(repository);
        }
        return repositories.OrderByDescending(r => r.UpdatedAt).ToList();
    }

    public async Task DeleteHackathonRepositoryAsync(Guid id)
    {
        // Delete the repository metadata blob
        var repoBlob = _repositoriesContainer.GetBlobClient($"{id}.json");
        await repoBlob.DeleteIfExistsAsync();

        // Delete all repo-files blobs under the {id}/ prefix
        var prefix = $"{id}/";
        await foreach (var blobItem in _repoFilesContainer.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, CancellationToken.None))
        {
            await _repoFilesContainer.GetBlobClient(blobItem.Name).DeleteIfExistsAsync();
        }
    }


    // -------------------------------------------------------------------------
    // Repo file-tree operations
    // Blobs are stored as: {repoId}/{normalised-relative-path}
    // Folder sentinels are stored as: {repoId}/{folderPath}/.folder (zero bytes)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates that <paramref name="relativePath"/> does not escape the repo
    /// prefix and normalises separators to forward-slash.
    /// Mirrors <c>ApiValidation.ValidateRepoPath</c> as a defence-in-depth
    /// layer inside the storage tier (API validation is the first gate; this
    /// is the last).
    /// </summary>
    private static string NormaliseRepoPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Path must not be empty.", nameof(relativePath));

        // Decode percent-encoded characters so paths constructed from URL segments
        // (e.g., %2F → /) are stored and retrieved with proper directory structure.
        relativePath = Uri.UnescapeDataString(relativePath);

        if (relativePath.Contains('\0'))
            throw new ArgumentException("Path must not contain null bytes.", nameof(relativePath));

        // Reject absolute paths: leading '/', '\\', or Windows drive letters (e.g. "C:")
        if (relativePath.StartsWith('/') || relativePath.StartsWith('\\') ||
            (relativePath.Length >= 2 && relativePath[1] == ':'))
            throw new ArgumentException("Path must be relative.", nameof(relativePath));

        var normalised = relativePath.Replace('\\', '/').Trim('/');

        if (normalised.Length == 0)
            throw new ArgumentException("Path must not be empty after normalisation.", nameof(relativePath));

        // Reject traversal segments and bare '.' segments
        foreach (var seg in normalised.Split('/'))
        {
            if (seg == "..")
                throw new InvalidOperationException("Path traversal detected.");
            if (seg == ".")
                throw new ArgumentException("Path must not contain '.' segments.", nameof(relativePath));
            if (string.IsNullOrWhiteSpace(seg))
                throw new ArgumentException("Path must not contain empty segments.", nameof(relativePath));
        }

        return normalised;
    }

    public async Task<List<RepoFileEntry>> ListRepoFilesAsync(Guid repoId)
    {
        var prefix = $"{repoId}/";
        var entries = new List<RepoFileEntry>();

        await foreach (var blobItem in _repoFilesContainer.GetBlobsAsync(
            BlobTraits.Metadata, BlobStates.None, prefix, default))
        {
            // Strip the repoId prefix to get the relative path
            var relPath = blobItem.Name[prefix.Length..];

            // Folder sentinels — surface as folder entries
            if (relPath.EndsWith("/.folder", StringComparison.Ordinal))
            {
                var folderPath = relPath[..^"/.folder".Length];
                var folderName = folderPath.Contains('/') ? folderPath[(folderPath.LastIndexOf('/') + 1)..] : folderPath;
                var updatedAt = blobItem.Properties.LastModified?.UtcDateTime ?? DateTime.UtcNow;
                entries.Add(new RepoFileEntry(folderPath, folderName, IsFolder: true, SizeBytes: null, updatedAt));
                continue;
            }

            var name = relPath.Contains('/') ? relPath[(relPath.LastIndexOf('/') + 1)..] : relPath;
            entries.Add(new RepoFileEntry(
                relPath,
                name,
                IsFolder: false,
                blobItem.Properties.ContentLength,
                blobItem.Properties.LastModified?.UtcDateTime ?? DateTime.UtcNow));
        }

        return entries.OrderBy(e => e.Path).ToList();
    }

    public async Task<RepoFileContent?> GetRepoFileAsync(Guid repoId, string path)
    {
        var normalised = NormaliseRepoPath(path);
        var blob = _repoFilesContainer.GetBlobClient($"{repoId}/{normalised}");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        var updatedAt = response.Value.Details.LastModified.UtcDateTime;
        return new RepoFileContent(normalised, response.Value.Content.ToString(), updatedAt);
    }

    public async Task UpsertRepoFileAsync(Guid repoId, string path, string content)
    {
        var normalised = NormaliseRepoPath(path);
        var blob = _repoFilesContainer.GetBlobClient($"{repoId}/{normalised}");
        await blob.UploadAsync(BinaryData.FromString(content), overwrite: true);
        var headers = new BlobHttpHeaders { ContentType = "text/plain; charset=utf-8" };
        await blob.SetHttpHeadersAsync(headers);
    }

    public async Task CreateRepoFolderAsync(Guid repoId, string folderPath)
    {
        var normalised = NormaliseRepoPath(folderPath);
        // Store a zero-byte sentinel blob so the folder appears in listings
        var sentinelBlob = _repoFilesContainer.GetBlobClient($"{repoId}/{normalised}/.folder");
        await sentinelBlob.UploadAsync(BinaryData.FromBytes([]), overwrite: true);
    }

    public async Task RenameRepoFileAsync(Guid repoId, string oldPath, string newPath)
    {
        var normOld = NormaliseRepoPath(oldPath);
        var normNew = NormaliseRepoPath(newPath);

        var src = _repoFilesContainer.GetBlobClient($"{repoId}/{normOld}");
        if (!await src.ExistsAsync())
            throw new FileNotFoundException($"Source file '{oldPath}' not found.");

        var dst = _repoFilesContainer.GetBlobClient($"{repoId}/{normNew}");
        if (await dst.ExistsAsync())
            throw new InvalidOperationException($"A file already exists at '{newPath}'.");

        // Azure Blob Storage has no server-side rename — copy then delete
        await dst.StartCopyFromUriAsync(src.Uri);
        // Wait for copy to complete
        BlobProperties props;
        do { props = (await dst.GetPropertiesAsync()).Value; }
        while (props.CopyStatus == CopyStatus.Pending);

        if (props.CopyStatus != CopyStatus.Success)
            throw new InvalidOperationException($"Blob copy failed with status: {props.CopyStatus}");

        await src.DeleteAsync();
    }

    public async Task RenameRepoFolderAsync(Guid repoId, string oldPath, string newPath)
    {
        var normOld = NormaliseRepoPath(oldPath);
        var normNew = NormaliseRepoPath(newPath);
        var prefix = $"{repoId}/{normOld}/";

        var blobs = new List<string>();
        await foreach (var blobItem in _repoFilesContainer.GetBlobsAsync(
            BlobTraits.None, BlobStates.None, prefix, default))
        {
            blobs.Add(blobItem.Name);
        }

        if (blobs.Count == 0)
            throw new DirectoryNotFoundException($"Source folder '{oldPath}' not found or is empty.");

        // Check the destination prefix is not already occupied
        var destPrefix = $"{repoId}/{normNew}/";
        var destCheck = _repoFilesContainer.GetBlobsAsync(
            BlobTraits.None, BlobStates.None, destPrefix, default);
        await foreach (var _ in destCheck)
            throw new InvalidOperationException($"A folder already exists at '{newPath}'.");

        // Copy every blob under the old prefix to the new prefix, then delete the originals
        foreach (var blobName in blobs)
        {
            var tail = blobName[prefix.Length..];
            var src = _repoFilesContainer.GetBlobClient(blobName);
            var dst = _repoFilesContainer.GetBlobClient($"{destPrefix}{tail}");

            await dst.StartCopyFromUriAsync(src.Uri);
            BlobProperties props;
            do { props = (await dst.GetPropertiesAsync()).Value; }
            while (props.CopyStatus == CopyStatus.Pending);

            if (props.CopyStatus != CopyStatus.Success)
                throw new InvalidOperationException($"Blob copy failed for '{blobName}'.");

            await src.DeleteAsync();
        }
    }

    // -------------------------------------------------------------------------

    public async Task SaveHackathonBriefAsync(HackathonBrief brief)
    {
        brief.UpdatedAt = DateTime.UtcNow;
        if (brief.Id == Guid.Empty)
            brief.Id = Guid.NewGuid();

        var blob = _briefsContainer.GetBlobClient($"{brief.Id}.json");
        var json = JsonSerializer.Serialize(brief, JsonOptions);
        await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);
    }

    public async Task<HackathonBrief?> GetHackathonBriefAsync(Guid id)
    {
        var blob = _briefsContainer.GetBlobClient($"{id}.json");
        if (!await blob.ExistsAsync()) return null;

        var response = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<HackathonBrief>(response.Value.Content.ToString(), JsonOptions);
    }

    public async Task<List<HackathonBrief>> ListHackathonBriefsAsync()
    {
        var briefs = new List<HackathonBrief>();
        await foreach (var blobItem in _briefsContainer.GetBlobsAsync())
        {
            var blob = _briefsContainer.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var brief = JsonSerializer.Deserialize<HackathonBrief>(response.Value.Content.ToString(), JsonOptions);
            if (brief is not null) briefs.Add(brief);
        }
        return briefs.OrderByDescending(b => b.UpdatedAt).ToList();
    }

    public async Task DeleteHackathonBriefAsync(Guid id)
    {
        var blob = _briefsContainer.GetBlobClient($"{id}.json");
        await blob.DeleteIfExistsAsync();
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

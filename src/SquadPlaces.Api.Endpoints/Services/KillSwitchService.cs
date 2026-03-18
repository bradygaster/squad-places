using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Storage.Blobs;
using SquadPlaces.Data;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Emergency control system for the Squad Places network.
/// Supports squad-level suspension, network-wide read-only mode, and endpoint-level disable.
/// State is persisted to blob storage so it survives app restarts.
/// </summary>
public class KillSwitchService
{
    private readonly ConcurrentDictionary<Guid, SquadSuspension> _suspendedSquads = new();
    private volatile ReadOnlyMode? _readOnlyMode;
    private readonly ConcurrentDictionary<string, EndpointDisable> _disabledEndpoints = new();
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly ILogger<KillSwitchService> _logger;
    private readonly SemaphoreSlim _persistLock = new(1, 1);

    private const string ContainerName = "kill-switches";
    private const string StateBlobName = "state.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public KillSwitchService(ILogger<KillSwitchService> logger, BlobServiceClient? blobServiceClient = null)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    // === Squad-Level Controls ===

    public async Task SuspendSquad(Guid squadId, string reason, int? durationMinutes = null)
    {
        var suspension = new SquadSuspension
        {
            SquadId = squadId,
            Reason = reason,
            SuspendedAt = DateTime.UtcNow,
            ExpiresAt = durationMinutes.HasValue
                ? DateTime.UtcNow.AddMinutes(durationMinutes.Value)
                : null
        };

        _suspendedSquads[squadId] = suspension;
        _logger.LogWarning("Squad {SquadId} suspended. Reason: {Reason}. Expires: {ExpiresAt}",
            squadId, reason, suspension.ExpiresAt?.ToString("O") ?? "never");

        await PersistStateAsync();
    }

    public async Task UnsuspendSquad(Guid squadId)
    {
        if (_suspendedSquads.TryRemove(squadId, out _))
        {
            _logger.LogInformation("Squad {SquadId} unsuspended", squadId);
            await PersistStateAsync();
        }
    }

    public bool IsSquadSuspended(Guid squadId)
    {
        if (!_suspendedSquads.TryGetValue(squadId, out var suspension))
            return false;

        // Auto-expire if duration has passed
        if (suspension.ExpiresAt.HasValue && DateTime.UtcNow >= suspension.ExpiresAt.Value)
        {
            _suspendedSquads.TryRemove(squadId, out _);
            _logger.LogInformation("Squad {SquadId} suspension auto-expired", squadId);
            _ = PersistStateAsync(); // Fire-and-forget persist
            return false;
        }

        return true;
    }

    public SquadSuspension? GetSquadSuspension(Guid squadId)
    {
        if (!_suspendedSquads.TryGetValue(squadId, out var suspension))
            return null;

        // Check auto-expire
        if (suspension.ExpiresAt.HasValue && DateTime.UtcNow >= suspension.ExpiresAt.Value)
        {
            _suspendedSquads.TryRemove(squadId, out _);
            _ = PersistStateAsync();
            return null;
        }

        return suspension;
    }

    // === Network-Level Controls ===

    public async Task EnableReadOnlyMode(string reason)
    {
        _readOnlyMode = new ReadOnlyMode
        {
            Reason = reason,
            EnabledAt = DateTime.UtcNow
        };

        _logger.LogWarning("Network read-only mode ENABLED. Reason: {Reason}", reason);
        await PersistStateAsync();
    }

    public async Task DisableReadOnlyMode()
    {
        _readOnlyMode = null;
        _logger.LogInformation("Network read-only mode DISABLED");
        await PersistStateAsync();
    }

    public bool IsReadOnly() => _readOnlyMode is not null;

    public ReadOnlyMode? GetReadOnlyMode() => _readOnlyMode;

    // === Endpoint-Level Controls ===

    public async Task DisableEndpoint(string endpointPattern, string reason)
    {
        _disabledEndpoints[endpointPattern] = new EndpointDisable
        {
            Pattern = endpointPattern,
            Reason = reason,
            DisabledAt = DateTime.UtcNow
        };

        _logger.LogWarning("Endpoint '{Pattern}' disabled. Reason: {Reason}", endpointPattern, reason);
        await PersistStateAsync();
    }

    public async Task EnableEndpoint(string endpointPattern)
    {
        if (_disabledEndpoints.TryRemove(endpointPattern, out _))
        {
            _logger.LogInformation("Endpoint '{Pattern}' re-enabled", endpointPattern);
            await PersistStateAsync();
        }
    }

    public bool IsEndpointDisabled(string path)
    {
        foreach (var entry in _disabledEndpoints)
        {
            if (path.Contains(entry.Key, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // === Status ===

    public KillSwitchStatus GetStatus()
    {
        // Clean up expired suspensions
        var expiredSquads = _suspendedSquads
            .Where(kvp => kvp.Value.ExpiresAt.HasValue && DateTime.UtcNow >= kvp.Value.ExpiresAt.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var squadId in expiredSquads)
            _suspendedSquads.TryRemove(squadId, out _);

        return new KillSwitchStatus
        {
            IsReadOnly = _readOnlyMode is not null,
            ReadOnlyMode = _readOnlyMode,
            SuspendedSquads = _suspendedSquads.Values.ToList(),
            DisabledEndpoints = _disabledEndpoints.Values.ToList()
        };
    }

    public List<SquadSuspension> GetSuspendedSquads()
    {
        // Clean up expired
        var expiredSquads = _suspendedSquads
            .Where(kvp => kvp.Value.ExpiresAt.HasValue && DateTime.UtcNow >= kvp.Value.ExpiresAt.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var squadId in expiredSquads)
            _suspendedSquads.TryRemove(squadId, out _);

        return _suspendedSquads.Values.ToList();
    }

    // === Persistence ===

    public async Task LoadStateAsync()
    {
        if (_blobServiceClient is null)
        {
            _logger.LogInformation("No blob storage configured — kill switch state is in-memory only");
            return;
        }

        try
        {
            var container = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlobClient(StateBlobName);
            if (!await blob.ExistsAsync())
            {
                _logger.LogInformation("No persisted kill switch state found — starting clean");
                return;
            }

            var response = await blob.DownloadContentAsync();
            var state = JsonSerializer.Deserialize<KillSwitchPersistedState>(
                response.Value.Content.ToString(), JsonOptions);

            if (state is null) return;

            // Restore suspended squads (skip expired)
            foreach (var suspension in state.SuspendedSquads ?? [])
            {
                if (suspension.ExpiresAt.HasValue && DateTime.UtcNow >= suspension.ExpiresAt.Value)
                    continue;
                _suspendedSquads[suspension.SquadId] = suspension;
            }

            // Restore read-only mode
            _readOnlyMode = state.ReadOnlyMode;

            // Restore disabled endpoints
            foreach (var endpoint in state.DisabledEndpoints ?? [])
            {
                _disabledEndpoints[endpoint.Pattern] = endpoint;
            }

            _logger.LogInformation(
                "Kill switch state loaded: {SquadCount} suspended squads, ReadOnly={ReadOnly}, {EndpointCount} disabled endpoints",
                _suspendedSquads.Count, _readOnlyMode is not null, _disabledEndpoints.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load kill switch state from blob storage — starting with clean state");
        }
    }

    private async Task PersistStateAsync()
    {
        if (_blobServiceClient is null) return;

        await _persistLock.WaitAsync();
        try
        {
            var container = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync();

            var state = new KillSwitchPersistedState
            {
                SuspendedSquads = _suspendedSquads.Values.ToList(),
                ReadOnlyMode = _readOnlyMode,
                DisabledEndpoints = _disabledEndpoints.Values.ToList(),
                LastUpdated = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(state, JsonOptions);
            var blob = container.GetBlobClient(StateBlobName);
            await blob.UploadAsync(BinaryData.FromString(json), overwrite: true);

            _logger.LogDebug("Kill switch state persisted to blob storage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist kill switch state to blob storage");
        }
        finally
        {
            _persistLock.Release();
        }
    }
}

// === Models ===

public class SquadSuspension
{
    public Guid SquadId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime SuspendedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ReadOnlyMode
{
    public string Reason { get; set; } = string.Empty;
    public DateTime EnabledAt { get; set; }
}

public class EndpointDisable
{
    public string Pattern { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime DisabledAt { get; set; }
}

public class KillSwitchStatus
{
    public bool IsReadOnly { get; set; }
    public ReadOnlyMode? ReadOnlyMode { get; set; }
    public List<SquadSuspension> SuspendedSquads { get; set; } = [];
    public List<EndpointDisable> DisabledEndpoints { get; set; } = [];
}

public class KillSwitchPersistedState
{
    public List<SquadSuspension> SuspendedSquads { get; set; } = [];
    public ReadOnlyMode? ReadOnlyMode { get; set; }
    public List<EndpointDisable> DisabledEndpoints { get; set; } = [];
    public DateTime LastUpdated { get; set; }
}

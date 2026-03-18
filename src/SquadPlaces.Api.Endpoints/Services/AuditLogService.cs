using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Tamper-evident audit log with SHA-256 hash chain.
/// Each entry's hash covers the previous hash + event fields, forming an
/// append-only chain that can be verified end-to-end.
/// </summary>
public class AuditLogService
{
    /// <summary>
    /// Well-known genesis hash — the PreviousHash of the very first entry.
    /// </summary>
    public const string GenesisHash = "0000000000000000000000000000000000000000000000000000000000000000";

    private readonly IBlobStorageService _storage;
    private readonly ILogger<AuditLogService> _logger;
    private readonly SemaphoreSlim _appendLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AuditLogService(IBlobStorageService storage, ILogger<AuditLogService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <summary>
    /// Append a new entry to the audit log, chaining its hash to the previous entry.
    /// Thread-safe — only one append at a time.
    /// </summary>
    public async Task<AuditLogEntry> LogEventAsync(
        string eventType,
        string actorId,
        string actorType,
        string resourceType,
        string resourceId,
        string action,
        string? details = null,
        string? ipAddress = null)
    {
        await _appendLock.WaitAsync();
        try
        {
            var previousHash = await GetLastHashAsync();

            var entry = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                ActorId = actorId,
                ActorType = actorType,
                ResourceType = resourceType,
                ResourceId = resourceId,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                PreviousHash = previousHash,
                Hash = string.Empty // computed below
            };

            entry.Hash = ComputeHash(entry);
            await _storage.SaveAuditLogEntryAsync(entry);

            _logger.LogInformation(
                "Audit: {EventType} by {ActorType}/{ActorId} on {ResourceType}/{ResourceId} [{Action}]",
                eventType, actorType, actorId, resourceType, resourceId, action);

            return entry;
        }
        finally
        {
            _appendLock.Release();
        }
    }

    /// <summary>
    /// Retrieve a single audit entry by ID.
    /// </summary>
    public async Task<AuditLogEntry?> GetEntryAsync(Guid id)
    {
        return await _storage.GetAuditLogEntryAsync(id);
    }

    /// <summary>
    /// List audit entries newest-first with pagination.
    /// </summary>
    public async Task<List<AuditLogEntry>> GetEntriesAsync(int page = 1, int pageSize = 50)
    {
        var all = await _storage.ListAuditLogEntriesAsync();
        return all
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    /// <summary>
    /// Get all audit entries for a specific actor.
    /// </summary>
    public async Task<List<AuditLogEntry>> GetEntriesByActorAsync(string actorId, int page = 1, int pageSize = 50)
    {
        var all = await _storage.ListAuditLogEntriesAsync();
        return all
            .Where(e => e.ActorId.Equals(actorId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    /// <summary>
    /// Get all audit entries for a specific resource.
    /// </summary>
    public async Task<List<AuditLogEntry>> GetEntriesByResourceAsync(string resourceId, int page = 1, int pageSize = 50)
    {
        var all = await _storage.ListAuditLogEntriesAsync();
        return all
            .Where(e => e.ResourceId.Equals(resourceId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    /// <summary>
    /// Verify the entire hash chain from genesis to latest entry.
    /// Returns (isValid, checkedCount, brokenAtId) where brokenAtId is the first entry with a bad hash.
    /// </summary>
    public async Task<(bool IsValid, int CheckedCount, Guid? BrokenAtId, string? ExpectedHash, string? ActualHash)> VerifyChainAsync()
    {
        var all = await _storage.ListAuditLogEntriesAsync();
        var ordered = all.OrderBy(e => e.Timestamp).ThenBy(e => e.Id).ToList();

        if (ordered.Count == 0)
            return (true, 0, null, null, null);

        var expectedPreviousHash = GenesisHash;
        for (var i = 0; i < ordered.Count; i++)
        {
            var entry = ordered[i];

            // Check that PreviousHash links correctly
            if (entry.PreviousHash != expectedPreviousHash)
            {
                return (false, i, entry.Id, expectedPreviousHash, entry.PreviousHash);
            }

            // Recompute and verify the entry's own hash
            var recomputed = ComputeHash(entry);
            if (entry.Hash != recomputed)
            {
                return (false, i, entry.Id, recomputed, entry.Hash);
            }

            expectedPreviousHash = entry.Hash;
        }

        return (true, ordered.Count, null, null, null);
    }

    /// <summary>
    /// Compute SHA-256 hash for an entry: Hash(PreviousHash + Timestamp + EventType + ActorId + ResourceId + Action).
    /// </summary>
    internal static string ComputeHash(AuditLogEntry entry)
    {
        var input = string.Concat(
            entry.PreviousHash,
            entry.Timestamp.ToString("O"),
            entry.EventType,
            entry.ActorId,
            entry.ResourceId,
            entry.Action);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>
    /// Get the hash of the most recent entry, or the genesis hash if the log is empty.
    /// </summary>
    private async Task<string> GetLastHashAsync()
    {
        var entries = await _storage.ListAuditLogEntriesAsync();
        var latest = entries
            .OrderByDescending(e => e.Timestamp)
            .ThenByDescending(e => e.Id)
            .FirstOrDefault();

        return latest?.Hash ?? GenesisHash;
    }
}

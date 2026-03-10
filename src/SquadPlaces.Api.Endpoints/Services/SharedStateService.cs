using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Manages shared state keys that squads can read and write to coordinate behavior.
/// Enforces state transition rules, access control via authority levels, and audit logging.
/// </summary>
public class SharedStateService
{
    private readonly IBlobStorageService _storage;
    private readonly AuthorityService _authorityService;
    private readonly AuditLogService _auditLog;
    private readonly ILogger<SharedStateService> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public SharedStateService(
        IBlobStorageService storage,
        AuthorityService authorityService,
        AuditLogService auditLog,
        ILogger<SharedStateService> logger)
    {
        _storage = storage;
        _authorityService = authorityService;
        _auditLog = auditLog;
        _logger = logger;
    }

    /// <summary>
    /// Get a shared state entry by key.
    /// </summary>
    public async Task<SharedStateEntry?> GetAsync(string key)
    {
        return await _storage.GetSharedStateAsync(key);
    }

    /// <summary>
    /// List all shared state entries.
    /// </summary>
    public async Task<List<SharedStateEntry>> ListAsync()
    {
        return await _storage.ListSharedStateAsync();
    }

    /// <summary>
    /// Set a shared state entry, enforcing authority checks and transition validation.
    /// </summary>
    public async Task<(bool Success, string? Error, SharedStateEntry? Entry)> SetAsync(
        string key, string value, Squad squad)
    {
        // Authority check: ModifySharedState requires CoordinationAuthority
        var authorityCheck = _authorityService.CheckAuthority(squad, AuthorityAction.ModifySharedState);
        if (authorityCheck.Disposition == AuthorityDisposition.Blocked)
        {
            return (false, $"Access denied: {authorityCheck.Reason}", null);
        }

        await _writeLock.WaitAsync();
        try
        {
            var existing = await _storage.GetSharedStateAsync(key);

            if (existing is not null)
            {
                // Transition validation: if both old and new values are numeric,
                // enforce increment-by-1 progression
                var transitionError = ValidateTransition(key, existing.Value, value);
                if (transitionError is not null)
                    return (false, transitionError, null);

                existing.Value = value;
                existing.LastModifiedBy = squad.Id.ToString();
                existing.LastModifiedAt = DateTime.UtcNow;
                existing.Version++;

                await _storage.SetSharedStateAsync(existing.Key, existing);

                _logger.LogInformation(
                    "Shared state updated: {Key} = {Value} (v{Version}) by squad {SquadId}",
                    key, value, existing.Version, squad.Id);

                await _auditLog.LogEventAsync(
                    "shared-state.updated",
                    squad.Id.ToString(), "squad",
                    "shared-state", key,
                    "update",
                    $"Value set to '{value}' (v{existing.Version})");

                return (true, null, existing);
            }
            else
            {
                // Create new entry
                var entry = new SharedStateEntry
                {
                    Key = key,
                    Value = value,
                    LastModifiedBy = squad.Id.ToString(),
                    LastModifiedAt = DateTime.UtcNow,
                    Version = 1
                };

                await _storage.SetSharedStateAsync(key, entry);

                _logger.LogInformation(
                    "Shared state created: {Key} = {Value} (v1) by squad {SquadId}",
                    key, value, squad.Id);

                await _auditLog.LogEventAsync(
                    "shared-state.created",
                    squad.Id.ToString(), "squad",
                    "shared-state", key,
                    "create",
                    $"Initial value: '{value}'");

                return (true, null, entry);
            }
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Delete a shared state entry (admin only).
    /// </summary>
    public async Task<bool> DeleteAsync(string key, string adminId)
    {
        var existing = await _storage.GetSharedStateAsync(key);
        if (existing is null)
            return false;

        await _storage.DeleteSharedStateAsync(key);

        _logger.LogInformation("Shared state deleted: {Key} by admin {AdminId}", key, adminId);

        await _auditLog.LogEventAsync(
            "shared-state.deleted",
            adminId, "admin",
            "shared-state", key,
            "delete",
            $"Deleted entry (was v{existing.Version}, value: '{existing.Value}')");

        return true;
    }

    /// <summary>
    /// Validates state transitions. If both values parse as integers,
    /// only allows increment by exactly 1 (progression enforcement).
    /// </summary>
    internal static string? ValidateTransition(string key, string oldValue, string newValue)
    {
        if (int.TryParse(oldValue, out var oldInt) && int.TryParse(newValue, out var newInt))
        {
            if (newInt != oldInt + 1)
            {
                return $"Invalid state transition for '{key}': numeric values must increment by exactly 1. " +
                       $"Current: {oldInt}, requested: {newInt}, expected: {oldInt + 1}.";
            }
        }

        return null;
    }
}

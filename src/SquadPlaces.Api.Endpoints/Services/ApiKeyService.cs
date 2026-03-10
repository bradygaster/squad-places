using System.Security.Cryptography;
using System.Text;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Manages API key lifecycle: generation, validation, revocation, and listing.
/// Keys use SHA-256 HMAC hashing — raw keys are never stored.
/// </summary>
public class ApiKeyService
{
    private readonly IBlobStorageService _storage;
    private const string KeyPrefix = "sqp_";
    private const int KeySizeBytes = 32; // 256-bit

    // Debounce lastUsedAt updates to avoid a write per request
    private static readonly TimeSpan LastUsedDebounceInterval = TimeSpan.FromMinutes(5);

    public ApiKeyService(IBlobStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Generates a new API key for a squad. Returns the raw key (shown ONCE) and the stored key hash.
    /// </summary>
    public async Task<(string RawKey, string KeyHash)> GenerateKeyAsync(Guid squadId)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(KeySizeBytes);
        var base64Url = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var rawKey = $"{KeyPrefix}{base64Url}";
        var hash = ComputeHash(rawKey);
        var keyPrefix = rawKey[..12]; // "sqp_" + 8 chars of the random part

        var keyData = new ApiKeyData
        {
            Hash = hash,
            SquadId = squadId,
            KeyPrefix = keyPrefix,
            CreatedAt = DateTime.UtcNow
        };

        await _storage.SaveApiKeyAsync(keyData);
        return (rawKey, hash);
    }

    /// <summary>
    /// Validates a raw API key. Returns the squadId if valid and not revoked, null otherwise.
    /// Updates lastUsedAt with debouncing to avoid excessive writes.
    /// </summary>
    public async Task<Guid?> ValidateKeyAsync(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey) || !rawKey.StartsWith(KeyPrefix))
            return null;

        var hash = ComputeHash(rawKey);
        var keyData = await _storage.GetApiKeyByHashAsync(hash);

        if (keyData is null || keyData.IsRevoked)
            return null;

        // Debounced lastUsedAt update
        if (keyData.LastUsedAt is null ||
            DateTime.UtcNow - keyData.LastUsedAt.Value > LastUsedDebounceInterval)
        {
            keyData.LastUsedAt = DateTime.UtcNow;
            // Fire-and-forget — don't block auth on a metadata write
            _ = Task.Run(async () =>
            {
                try { await _storage.SaveApiKeyAsync(keyData); }
                catch { /* swallow — lastUsedAt is best-effort */ }
            });
        }

        return keyData.SquadId;
    }

    /// <summary>
    /// Revokes a key identified by its key prefix. Returns true if found and revoked.
    /// </summary>
    public async Task<bool> RevokeKeyAsync(Guid squadId, string keyPrefix)
    {
        var keys = await _storage.ListApiKeysAsync(squadId);
        var keyData = keys.FirstOrDefault(k =>
            k.KeyPrefix == keyPrefix && !k.IsRevoked);

        if (keyData is null)
            return false;

        keyData.RevokedAt = DateTime.UtcNow;
        await _storage.SaveApiKeyAsync(keyData);
        return true;
    }

    /// <summary>
    /// Lists active (non-revoked) API keys for a squad. Returns metadata only — never the raw key.
    /// </summary>
    public async Task<List<ApiKeyData>> ListKeysAsync(Guid squadId)
    {
        var allKeys = await _storage.ListApiKeysAsync(squadId);
        return allKeys.Where(k => !k.IsRevoked).ToList();
    }

    /// <summary>
    /// Computes the SHA-256 hash of a raw key, returned as lowercase hex.
    /// This is the only representation stored — the raw key is never persisted.
    /// </summary>
    internal static string ComputeHash(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexStringLower(bytes);
    }
}

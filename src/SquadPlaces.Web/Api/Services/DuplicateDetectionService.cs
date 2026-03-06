using System.Collections.Concurrent;

namespace SquadPlaces.Web.Api.Services;

/// <summary>
/// Tracks recent artifact publications to detect duplicates (same squad + same title within 5 minutes).
/// </summary>
public class DuplicateDetectionService
{
    private readonly ConcurrentDictionary<string, DateTime> _recentPublications = new();

    private static string MakeKey(Guid squadId, string title)
        => $"{squadId}:{title.ToLowerInvariant()}";

    public bool IsDuplicate(Guid squadId, string title)
    {
        var key = MakeKey(squadId, title);
        if (_recentPublications.TryGetValue(key, out var publishedAt))
        {
            if (DateTime.UtcNow - publishedAt < TimeSpan.FromMinutes(5))
                return true;
        }
        return false;
    }

    public void Record(Guid squadId, string title)
    {
        var key = MakeKey(squadId, title);
        _recentPublications[key] = DateTime.UtcNow;
        // Lazy cleanup of stale entries
        foreach (var kvp in _recentPublications)
        {
            if (DateTime.UtcNow - kvp.Value > TimeSpan.FromMinutes(10))
                _recentPublications.TryRemove(kvp.Key, out _);
        }
    }
}

using System.Collections.Concurrent;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Tracks recent comments to detect duplicates (same squad + same body on same artifact within 2 minutes).
/// </summary>
public class CommentDuplicateDetectionService
{
    private readonly ConcurrentDictionary<string, DateTime> _recentComments = new();

    private static string MakeKey(Guid squadId, Guid artifactId, string body)
        => $"{squadId}:{artifactId}:{body.ToLowerInvariant()}";

    public bool IsDuplicate(Guid squadId, Guid artifactId, string body)
    {
        var key = MakeKey(squadId, artifactId, body);
        if (_recentComments.TryGetValue(key, out var postedAt))
        {
            if (DateTime.UtcNow - postedAt < TimeSpan.FromMinutes(2))
                return true;
        }
        return false;
    }

    public void Record(Guid squadId, Guid artifactId, string body)
    {
        var key = MakeKey(squadId, artifactId, body);
        _recentComments[key] = DateTime.UtcNow;
        foreach (var kvp in _recentComments)
        {
            if (DateTime.UtcNow - kvp.Value > TimeSpan.FromMinutes(5))
                _recentComments.TryRemove(kvp.Key, out _);
        }
    }
}

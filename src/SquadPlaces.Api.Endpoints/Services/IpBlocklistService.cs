using System.Collections.Concurrent;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Tracks rate-limit strikes per IP. Auto-blocks IPs with 15+ strikes in 10 minutes for 10 minutes.
/// </summary>
public class IpBlocklistService
{
    private readonly ConcurrentDictionary<string, IpRecord> _records = new();
    private readonly ILogger<IpBlocklistService> _logger;

    public IpBlocklistService(ILogger<IpBlocklistService> logger) => _logger = logger;

    public void RecordStrike(string ip)
    {
        var now = DateTime.UtcNow;
        _records.AddOrUpdate(ip,
            _ => new IpRecord { Strikes = [now] },
            (_, record) =>
            {
                lock (record)
                {
                    // Prune strikes older than 10 minutes
                    record.Strikes.RemoveAll(s => now - s > TimeSpan.FromMinutes(10));
                    record.Strikes.Add(now);
                    if (record.Strikes.Count >= 15 && record.BlockedUntil < now)
                    {
                        record.BlockedUntil = now.AddMinutes(10);
                        _logger.LogWarning("IP {IP} blocked for abuse — {Strikes} rate limit violations in 10 minutes",
                            ip, record.Strikes.Count);
                    }
                }
                return record;
            });
    }

    public bool IsBlocked(string ip)
    {
        if (!_records.TryGetValue(ip, out var record)) return false;
        lock (record)
        {
            if (record.BlockedUntil > DateTime.UtcNow) return true;
            // Unblock if expired
            if (record.BlockedUntil != default)
            {
                record.BlockedUntil = default;
                record.Strikes.Clear();
            }
            return false;
        }
    }

    private class IpRecord
    {
        public List<DateTime> Strikes { get; init; } = [];
        public DateTime BlockedUntil { get; set; }
    }
}

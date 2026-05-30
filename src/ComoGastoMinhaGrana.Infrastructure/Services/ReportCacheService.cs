using ComoGastoMinhaGrana.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ComoGastoMinhaGrana.Infrastructure.Services;

public class ReportCacheService : IReportCacheService
{
    private readonly IDistributedCache _cache;

    public ReportCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetAsync(Guid reportId)
        => await _cache.GetStringAsync(CacheKey(reportId));

    public async Task SetAsync(Guid reportId, string content, TimeSpan? ttl = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromHours(1)
        };
        await _cache.SetStringAsync(CacheKey(reportId), content, options);
    }

    public async Task RemoveAsync(Guid reportId)
        => await _cache.RemoveAsync(CacheKey(reportId));

    private static string CacheKey(Guid reportId) => $"report:{reportId}";
}

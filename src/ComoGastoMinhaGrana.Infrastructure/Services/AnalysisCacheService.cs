using ComoGastoMinhaGrana.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ComoGastoMinhaGrana.Infrastructure.Services;

public class AnalysisCacheService : IAnalysisCacheService
{
    private readonly IDistributedCache _cache;

    public AnalysisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetAsync(Guid statementId)
    {
        return await _cache.GetStringAsync(CacheKey(statementId));
    }

    public async Task SetAsync(Guid statementId, string content, TimeSpan? ttl = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromDays(7)
        };
        await _cache.SetStringAsync(CacheKey(statementId), content, options);
    }

    public async Task RemoveAsync(Guid statementId)
        => await _cache.RemoveAsync(CacheKey(statementId));

    private static string CacheKey(Guid statementId) => $"analysis:{statementId}";
}

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IReportCacheService
{
    Task<string?> GetAsync(Guid reportId);
    Task SetAsync(Guid reportId, string content, TimeSpan? ttl = null);
    Task RemoveAsync(Guid reportId);
}

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IAnalysisCacheService
{
    Task<string?> GetAsync(Guid statementId);
    Task SetAsync(Guid statementId, string content, TimeSpan? ttl = null);
    Task RemoveAsync(Guid statementId);
}

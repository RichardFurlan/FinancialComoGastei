using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<Report>> GetByUserIdAsync(Guid userId);
    Task<Report?> GetByIdWithStatementsAsync(Guid id);
    Task<bool> BelongsToUserAsync(Guid id, Guid userId);
    Task AddAsync(Report report);
    Task DeleteAsync(Report report);
}

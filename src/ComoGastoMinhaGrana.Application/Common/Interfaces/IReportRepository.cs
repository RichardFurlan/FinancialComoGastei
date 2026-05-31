using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<ReportSummaryDto>> GetSummariesByUserIdAsync(Guid userId);
    Task<IEnumerable<Report>> GetByUserIdAsync(Guid userId);
    Task<Report?> GetByIdWithStatementsAsync(Guid id);
    Task<bool> BelongsToUserAsync(Guid id, Guid userId);
    Task AddAsync(Report report);
    Task DeleteAsync(Report report);
}

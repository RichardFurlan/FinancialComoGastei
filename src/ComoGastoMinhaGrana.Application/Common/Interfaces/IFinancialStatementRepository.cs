using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IFinancialStatementRepository
{
    Task<FinancialStatement?> GetByIdAsync(Guid id);
    Task<FinancialStatement?> GetByIdAndUserIdAsync(Guid id, Guid userId);
    Task<IEnumerable<FinancialStatement>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<FinancialStatement>> GetAllAsync();
    Task AddAsync(FinancialStatement statement);
    Task UpdateAsync(FinancialStatement statement);
    Task DeleteAsync(FinancialStatement statement);
}

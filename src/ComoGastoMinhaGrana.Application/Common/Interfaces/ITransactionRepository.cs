using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByIdWithStatementAsync(Guid id);
    Task<IEnumerable<Transaction>> GetByStatementIdAsync(Guid statementId);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task UpdateAsync(Transaction transaction);
    Task UpdateRangeAsync(IEnumerable<Transaction> transactions);
}

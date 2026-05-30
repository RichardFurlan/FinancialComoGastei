using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ComoGastoMinhaGrana.Infrastructure.Persistence;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaction?> GetByIdWithStatementAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.FinancialStatement)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetByStatementIdAsync(Guid statementId)
    {
        return await _context.Transactions
            .Where(t => t.FinancialStatementId == statementId)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Transaction> transactions)
    {
        _context.Transactions.UpdateRange(transactions);
        await _context.SaveChangesAsync();
    }
}

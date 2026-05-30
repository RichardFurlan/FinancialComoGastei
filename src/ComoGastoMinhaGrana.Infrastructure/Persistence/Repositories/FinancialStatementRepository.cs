using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Repositories;

public class FinancialStatementRepository : IFinancialStatementRepository
{
    private readonly ApplicationDbContext _context;

    public FinancialStatementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialStatement?> GetByIdAsync(Guid id)
    {
        return await _context.FinancialStatements
            .Include(fs => fs.Transactions).ThenInclude(t => t.Category)
            .Include(fs => fs.Analysis)
            .FirstOrDefaultAsync(fs => fs.Id == id);
    }

    public async Task<FinancialStatement?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return await _context.FinancialStatements
            .Include(fs => fs.Transactions).ThenInclude(t => t.Category)
            .Include(fs => fs.Analysis)
            .FirstOrDefaultAsync(fs => fs.Id == id && fs.UserId == userId);
    }

    public async Task<IEnumerable<FinancialStatement>> GetByUserIdAsync(Guid userId)
    {
        return await _context.FinancialStatements
            .Include(fs => fs.Transactions)
            .Include(fs => fs.Analysis)
            .Where(fs => fs.UserId == userId)
            .OrderByDescending(fs => fs.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialStatement>> GetAllAsync()
    {
        return await _context.FinancialStatements
            .Include(fs => fs.Transactions)
            .Include(fs => fs.Analysis)
            .ToListAsync();
    }

    public async Task AddAsync(FinancialStatement statement)
    {
        await _context.FinancialStatements.AddAsync(statement);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FinancialStatement statement)
    {
        _context.FinancialStatements.Update(statement);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(FinancialStatement statement)
    {
        _context.FinancialStatements.Remove(statement);
        await _context.SaveChangesAsync();
    }
}

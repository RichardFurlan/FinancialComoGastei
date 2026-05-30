using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Report>> GetByUserIdAsync(Guid userId)
        => await _context.Reports
            .Include(r => r.Statements)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<Report?> GetByIdWithStatementsAsync(Guid id)
        => await _context.Reports
            .Include(r => r.Statements)
                .ThenInclude(rs => rs.Statement)
                    .ThenInclude(s => s.Transactions)
                        .ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<bool> BelongsToUserAsync(Guid id, Guid userId)
        => await _context.Reports.AnyAsync(r => r.Id == id && r.UserId == userId);

    public async Task AddAsync(Report report)
    {
        await _context.Reports.AddAsync(report);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Report report)
    {
        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
    }
}

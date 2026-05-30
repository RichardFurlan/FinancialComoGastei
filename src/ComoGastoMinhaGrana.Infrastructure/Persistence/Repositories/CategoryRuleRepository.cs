using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Repositories;

public class CategoryRuleRepository : ICategoryRuleRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryRule?> GetByIdAsync(Guid id)
        => await _context.CategoryRules.Include(r => r.Category).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<CategoryRule>> GetByUserIdAsync(Guid userId)
        => await _context.CategoryRules
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(CategoryRule rule)
    {
        await _context.CategoryRules.AddAsync(rule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategoryRule rule)
    {
        _context.CategoryRules.Update(rule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(CategoryRule rule)
    {
        _context.CategoryRules.Remove(rule);
        await _context.SaveChangesAsync();
    }
}

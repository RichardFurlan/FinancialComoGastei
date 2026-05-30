using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id)
        => await _context.Categories.FindAsync(id);

    public async Task<IEnumerable<Category>> GetAllByUserIdAsync(Guid userId)
        => await _context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<bool> ExistsByNameAsync(string name, Guid userId, Guid? excludeId = null)
        => await _context.Categories.AnyAsync(c =>
            c.UserId == userId &&
            c.Name.ToLower() == name.ToLower() &&
            c.Id != excludeId);

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}

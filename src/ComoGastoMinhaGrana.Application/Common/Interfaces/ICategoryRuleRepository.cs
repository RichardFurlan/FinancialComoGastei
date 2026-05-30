using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface ICategoryRuleRepository
{
    Task<CategoryRule?> GetByIdAsync(Guid id);
    Task<IEnumerable<CategoryRule>> GetByUserIdAsync(Guid userId);
    Task AddAsync(CategoryRule rule);
    Task UpdateAsync(CategoryRule rule);
    Task DeleteAsync(CategoryRule rule);
}

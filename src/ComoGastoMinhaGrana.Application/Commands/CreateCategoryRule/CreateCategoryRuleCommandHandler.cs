using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.CreateCategoryRule;

public class CreateCategoryRuleCommandHandler : IRequestHandler<CreateCategoryRuleCommand, CategoryRuleDto>
{
    private readonly ICategoryRuleRepository _ruleRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryRuleCommandHandler(ICategoryRuleRepository ruleRepository, ICategoryRepository categoryRepository)
    {
        _ruleRepository = ruleRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryRuleDto> Handle(CreateCategoryRuleCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || category.UserId != request.UserId)
            throw new InvalidOperationException("Categoria não encontrada.");

        var existingRules = await _ruleRepository.GetByUserIdAsync(request.UserId);
        var nextPriority = existingRules.Count() + 1;

        var rule = new CategoryRule
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SearchTerm = request.SearchTerm.Trim(),
            RuleMatchType = request.RuleMatchType,
            CategoryId = request.CategoryId,
            Priority = nextPriority,
            CreatedAt = DateTime.UtcNow
        };

        await _ruleRepository.AddAsync(rule);

        return new CategoryRuleDto(rule.Id, rule.SearchTerm, rule.RuleMatchType.ToString(),
            category.Id, category.Name, category.Color, rule.Priority);
    }
}

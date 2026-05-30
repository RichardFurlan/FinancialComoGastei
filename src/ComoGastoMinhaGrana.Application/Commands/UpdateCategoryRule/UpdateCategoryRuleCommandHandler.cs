using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.UpdateCategoryRule;

public class UpdateCategoryRuleCommandHandler : IRequestHandler<UpdateCategoryRuleCommand, UpdateCategoryRuleResult>
{
    private readonly ICategoryRuleRepository _ruleRepository;
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryRuleCommandHandler(ICategoryRuleRepository ruleRepository, ICategoryRepository categoryRepository)
    {
        _ruleRepository = ruleRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<UpdateCategoryRuleResult> Handle(UpdateCategoryRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _ruleRepository.GetByIdAsync(request.Id);
        if (rule is null)
            return new UpdateCategoryRuleResult(null, UpdateCategoryRuleError.NotFound);

        if (rule.UserId != request.UserId)
            return new UpdateCategoryRuleResult(null, UpdateCategoryRuleError.Forbidden);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || category.UserId != request.UserId)
            return new UpdateCategoryRuleResult(null, UpdateCategoryRuleError.NotFound);

        rule.SearchTerm = request.SearchTerm.Trim();
        rule.RuleMatchType = request.RuleMatchType;
        rule.CategoryId = request.CategoryId;

        await _ruleRepository.UpdateAsync(rule);

        return new UpdateCategoryRuleResult(
            new CategoryRuleDto(rule.Id, rule.SearchTerm, rule.RuleMatchType.ToString(),
                category.Id, category.Name, category.Color, rule.Priority));
    }
}

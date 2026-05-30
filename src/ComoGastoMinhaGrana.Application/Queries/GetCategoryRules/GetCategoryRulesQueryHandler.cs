using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetCategoryRules;

public class GetCategoryRulesQueryHandler : IRequestHandler<GetCategoryRulesQuery, IEnumerable<CategoryRuleDto>>
{
    private readonly ICategoryRuleRepository _repository;

    public GetCategoryRulesQueryHandler(ICategoryRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoryRuleDto>> Handle(GetCategoryRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.GetByUserIdAsync(request.UserId);
        return rules.Select(r => new CategoryRuleDto(
            r.Id,
            r.SearchTerm,
            r.RuleMatchType.ToString(),
            r.CategoryId,
            r.Category.Name,
            r.Category.Color,
            r.Priority));
    }
}

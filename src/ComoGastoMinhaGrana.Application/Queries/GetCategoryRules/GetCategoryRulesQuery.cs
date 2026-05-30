using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetCategoryRules;

public record GetCategoryRulesQuery(Guid UserId) : IRequest<IEnumerable<CategoryRuleDto>>;

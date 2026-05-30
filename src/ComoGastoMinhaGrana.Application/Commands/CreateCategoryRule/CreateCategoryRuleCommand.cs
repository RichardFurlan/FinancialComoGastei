using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Domain.Enums;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.CreateCategoryRule;

public record CreateCategoryRuleCommand(
    Guid UserId,
    string SearchTerm,
    RuleMatchType RuleMatchType,
    Guid CategoryId) : IRequest<CategoryRuleDto>;

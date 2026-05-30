using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Domain.Enums;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.UpdateCategoryRule;

public record UpdateCategoryRuleCommand(
    Guid Id,
    Guid UserId,
    string SearchTerm,
    RuleMatchType RuleMatchType,
    Guid CategoryId) : IRequest<UpdateCategoryRuleResult>;

public record UpdateCategoryRuleResult(CategoryRuleDto? Rule, UpdateCategoryRuleError Error = UpdateCategoryRuleError.None);
public enum UpdateCategoryRuleError { None, NotFound, Forbidden }

namespace ComoGastoMinhaGrana.Application.Common.Dtos;

public record CategoryRuleDto(
    Guid Id,
    string SearchTerm,
    string RuleMatchType,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    int Priority);

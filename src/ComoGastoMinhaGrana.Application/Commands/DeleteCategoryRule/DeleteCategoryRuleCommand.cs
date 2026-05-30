using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteCategoryRule;

public record DeleteCategoryRuleCommand(Guid Id, Guid UserId) : IRequest<DeleteCategoryRuleError>;
public enum DeleteCategoryRuleError { None, NotFound, Forbidden }

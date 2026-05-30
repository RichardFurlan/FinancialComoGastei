using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.ApplyRulesToStatement;

public record ApplyRulesToStatementCommand(Guid StatementId, Guid UserId) : IRequest<ApplyRulesResult>;

public record ApplyRulesResult(int CategorizedCount, ApplyRulesError Error = ApplyRulesError.None);
public enum ApplyRulesError { None, NotFound, Forbidden }

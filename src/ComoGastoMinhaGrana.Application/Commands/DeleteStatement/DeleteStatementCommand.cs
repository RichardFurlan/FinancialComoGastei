using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteStatement;

public record DeleteStatementCommand(Guid StatementId, Guid UserId) : IRequest<DeleteStatementError>;

public enum DeleteStatementError { None, NotFound, Forbidden, Processing }

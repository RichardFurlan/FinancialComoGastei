using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetStatementAnalysis;

public record GetStatementAnalysisQuery(Guid StatementId, Guid UserId) : IRequest<string?>;

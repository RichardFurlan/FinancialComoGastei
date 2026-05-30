using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetStatements;

public record GetStatementsQuery(Guid UserId) : IRequest<List<StatementSummaryDto>>;

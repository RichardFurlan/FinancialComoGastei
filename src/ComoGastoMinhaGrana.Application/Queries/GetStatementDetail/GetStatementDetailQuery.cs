using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetStatementDetail;

public record GetStatementDetailQuery(Guid StatementId, Guid UserId) : IRequest<StatementDetailDto?>;

using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetReportDetail;

public record GetReportDetailQuery(Guid Id, Guid UserId) : IRequest<ReportDetailDto?>;

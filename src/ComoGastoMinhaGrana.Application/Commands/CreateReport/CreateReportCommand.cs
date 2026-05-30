using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.CreateReport;

public record CreateReportCommand(Guid UserId, string Name, IList<Guid> StatementIds)
    : IRequest<CreateReportResult>;

public record CreateReportResult(ReportSummaryDto? Report, CreateReportError Error = CreateReportError.None);
public enum CreateReportError { None, TooManyStatements, NoStatements }

using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteReport;

public record DeleteReportCommand(Guid Id, Guid UserId) : IRequest<DeleteReportError>;
public enum DeleteReportError { None, NotFound, Forbidden }

using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteReport;

public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, DeleteReportError>
{
    private readonly IReportRepository _repository;
    private readonly IReportCacheService _cache;

    public DeleteReportCommandHandler(IReportRepository repository, IReportCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<DeleteReportError> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdWithStatementsAsync(request.Id);
        if (report is null) return DeleteReportError.NotFound;
        if (report.UserId != request.UserId) return DeleteReportError.Forbidden;

        await _repository.DeleteAsync(report);
        await _cache.RemoveAsync(request.Id);

        return DeleteReportError.None;
    }
}

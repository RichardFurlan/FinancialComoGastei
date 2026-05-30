using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteStatement;

public class DeleteStatementCommandHandler : IRequestHandler<DeleteStatementCommand, DeleteStatementError>
{
    private readonly IFinancialStatementRepository _statementRepository;
    private readonly IAnalysisCacheService _cache;
    private readonly ILogger<DeleteStatementCommandHandler> _logger;

    public DeleteStatementCommandHandler(
        IFinancialStatementRepository statementRepository,
        IAnalysisCacheService cache,
        ILogger<DeleteStatementCommandHandler> logger)
    {
        _statementRepository = statementRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DeleteStatementError> Handle(DeleteStatementCommand request, CancellationToken cancellationToken)
    {
        var statement = await _statementRepository.GetByIdAndUserIdAsync(request.StatementId, request.UserId);
        if (statement is null)
            return DeleteStatementError.NotFound;

        if (statement.UserId != request.UserId)
            return DeleteStatementError.Forbidden;

        if (statement.Status == StatementStatus.Processing)
            return DeleteStatementError.Processing;

        await _statementRepository.DeleteAsync(statement);
        await _cache.RemoveAsync(request.StatementId);

        _logger.LogInformation("Extrato {StatementId} deletado pelo usuário {UserId}", request.StatementId, request.UserId);

        return DeleteStatementError.None;
    }
}

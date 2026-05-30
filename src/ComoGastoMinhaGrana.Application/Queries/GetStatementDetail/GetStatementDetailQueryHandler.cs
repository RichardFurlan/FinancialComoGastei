using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetStatementDetail;

public class GetStatementDetailQueryHandler : IRequestHandler<GetStatementDetailQuery, StatementDetailDto?>
{
    private readonly IFinancialStatementRepository _repository;

    public GetStatementDetailQueryHandler(IFinancialStatementRepository repository)
    {
        _repository = repository;
    }

    public async Task<StatementDetailDto?> Handle(GetStatementDetailQuery request, CancellationToken cancellationToken)
    {
        var statement = await _repository.GetByIdAndUserIdAsync(request.StatementId, request.UserId);
        if (statement is null) return null;

        var transactions = statement.Transactions
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionDto(
                t.Id,
                t.Date,
                t.OriginalDescription,
                t.Amount,
                t.Currency,
                t.CategoryId,
                t.Category?.Name,
                t.Category?.Color))
            .ToList();

        return new StatementDetailDto(
            statement.Id,
            statement.FileName,
            statement.FileExtension,
            statement.UploadDate,
            statement.Status.ToString(),
            statement.BaseCurrency,
            statement.ErrorMessage,
            transactions,
            statement.Analysis is not null);
    }
}

using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.AssignCategory;

public class AssignCategoryCommandHandler : IRequestHandler<AssignCategoryCommand, AssignCategoryResult>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public AssignCategoryCommandHandler(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<AssignCategoryResult> Handle(AssignCategoryCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdWithStatementAsync(request.TransactionId);
        if (transaction is null)
            return AssignCategoryResult.TransactionNotFound;

        if (transaction.FinancialStatement.UserId != request.UserId)
            return AssignCategoryResult.Forbidden;

        if (request.CategoryId is not null)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
            if (category is null || category.UserId != request.UserId)
                return AssignCategoryResult.CategoryNotFound;
        }

        transaction.CategoryId = request.CategoryId;
        await _transactionRepository.UpdateAsync(transaction);

        return AssignCategoryResult.Ok;
    }
}

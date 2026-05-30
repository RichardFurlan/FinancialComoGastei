using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteCategoryRule;

public class DeleteCategoryRuleCommandHandler : IRequestHandler<DeleteCategoryRuleCommand, DeleteCategoryRuleError>
{
    private readonly ICategoryRuleRepository _repository;

    public DeleteCategoryRuleCommandHandler(ICategoryRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteCategoryRuleError> Handle(DeleteCategoryRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.Id);
        if (rule is null) return DeleteCategoryRuleError.NotFound;
        if (rule.UserId != request.UserId) return DeleteCategoryRuleError.Forbidden;

        await _repository.DeleteAsync(rule);
        return DeleteCategoryRuleError.None;
    }
}

using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryError>
{
    private readonly ICategoryRepository _repository;

    public DeleteCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteCategoryError> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category is null)
            return DeleteCategoryError.NotFound;

        if (category.UserId != request.UserId)
            return DeleteCategoryError.Forbidden;

        await _repository.DeleteAsync(category);
        return DeleteCategoryError.None;
    }
}

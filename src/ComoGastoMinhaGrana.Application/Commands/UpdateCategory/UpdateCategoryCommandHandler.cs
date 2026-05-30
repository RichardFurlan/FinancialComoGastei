using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResult>
{
    private readonly ICategoryRepository _repository;

    public UpdateCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category is null)
            return new UpdateCategoryResult(null, UpdateCategoryError.NotFound);

        if (category.UserId != request.UserId)
            return new UpdateCategoryResult(null, UpdateCategoryError.Forbidden);

        if (await _repository.ExistsByNameAsync(request.Name.Trim(), request.UserId, excludeId: request.Id))
            return new UpdateCategoryResult(null, UpdateCategoryError.DuplicateName);

        category.Name = request.Name.Trim();
        category.Color = request.Color;

        await _repository.UpdateAsync(category);

        return new UpdateCategoryResult(new CategoryDto(category.Id, category.Name, category.Color));
    }
}

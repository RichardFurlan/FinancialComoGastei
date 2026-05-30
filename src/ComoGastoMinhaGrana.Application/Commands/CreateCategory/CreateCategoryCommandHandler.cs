using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByNameAsync(request.Name.Trim(), request.UserId))
            return new CreateCategoryResult(null, CreateCategoryError.DuplicateName);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Color = request.Color
        };

        await _repository.AddAsync(category);

        return new CreateCategoryResult(new CategoryDto(category.Id, category.Name, category.Color));
    }
}

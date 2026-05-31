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

        try
        {
            await _repository.AddAsync(category);
        }
        catch (Exception ex) when (
            ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
            ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
            ex.InnerException?.Message.Contains("23505") == true)
        {
            return new CreateCategoryResult(null, CreateCategoryError.DuplicateName);
        }

        return new CreateCategoryResult(new CategoryDto(category.Id, category.Name, category.Color));
    }
}

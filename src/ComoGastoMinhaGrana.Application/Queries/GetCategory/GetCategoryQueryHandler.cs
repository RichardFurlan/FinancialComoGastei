using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetCategory;

public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, CategoryDto?>
{
    private readonly ICategoryRepository _repository;

    public GetCategoryQueryHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryDto?> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category is null || category.UserId != request.UserId)
            return null;

        return new CategoryDto(category.Id, category.Name, category.Color);
    }
}

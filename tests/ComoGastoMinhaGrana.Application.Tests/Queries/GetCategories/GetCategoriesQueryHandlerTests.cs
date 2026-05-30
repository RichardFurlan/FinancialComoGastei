using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Queries.GetCategories;
using ComoGastoMinhaGrana.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ComoGastoMinhaGrana.Application.Tests.Queries.GetCategories;

public class GetCategoriesQueryHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _handler = new GetCategoriesQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyCategoriesForUser()
    {
        var userId = Guid.NewGuid();
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Alimentação", Color = "#FF0000", UserId = userId },
            new() { Id = Guid.NewGuid(), Name = "Moradia",     Color = "#00FF00", UserId = userId }
        };

        _repository.GetAllByUserIdAsync(userId).Returns(categories);

        var result = (await _handler.Handle(new GetCategoriesQuery(userId), CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().BeEquivalentTo("Alimentação", "Moradia");
    }

    [Fact]
    public async Task Handle_WhenNoCategories_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _repository.GetAllByUserIdAsync(userId).Returns(Enumerable.Empty<Category>());

        var result = await _handler.Handle(new GetCategoriesQuery(userId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

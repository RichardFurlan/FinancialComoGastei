using ComoGastoMinhaGrana.Application.Commands.CreateCategory;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Tests.Common.Mothers;
using ComoGastoMinhaGrana.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ComoGastoMinhaGrana.Application.Tests.Commands.CreateCategory;

public class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenNameIsUnique_ReturnsCategoryDto()
    {
        var userId = Guid.NewGuid();
        var command = new CreateCategoryCommand(userId, "Alimentação", "#FF5733");

        _repository.ExistsByNameAsync("Alimentação", userId).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(CreateCategoryError.None);
        result.Category.Should().NotBeNull();
        result.Category!.Name.Should().Be("Alimentação");
        result.Category.Color.Should().Be("#FF5733");
        await _repository.Received(1).AddAsync(Arg.Is<Category>(c =>
            c.Name == "Alimentação" && c.Color == "#FF5733" && c.UserId == userId));
    }

    [Fact]
    public async Task Handle_WhenNameExists_ReturnsDuplicateName()
    {
        var userId = Guid.NewGuid();
        var command = new CreateCategoryCommand(userId, "Alimentação", "#FF5733");

        _repository.ExistsByNameAsync("Alimentação", userId).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(CreateCategoryError.DuplicateName);
        result.Category.Should().BeNull();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_TrimsWhitespaceFromName()
    {
        var userId = Guid.NewGuid();
        var command = new CreateCategoryCommand(userId, "  Lazer  ", "#000000");

        _repository.ExistsByNameAsync("Lazer", userId).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Category!.Name.Should().Be("Lazer");
    }
}

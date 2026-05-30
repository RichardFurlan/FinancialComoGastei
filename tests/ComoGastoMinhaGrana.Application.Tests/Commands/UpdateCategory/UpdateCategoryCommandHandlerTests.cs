using ComoGastoMinhaGrana.Application.Commands.UpdateCategory;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Tests.Common.Mothers;
using ComoGastoMinhaGrana.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ComoGastoMinhaGrana.Application.Tests.Commands.UpdateCategory;

public class UpdateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenOwnerAndNameUnique_UpdatesAndReturnsDto()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = CategoryMother.OwnedBy(userId, categoryId);
        var command = new UpdateCategoryCommand(categoryId, userId, "Novo", "#FFFFFF");

        _repository.GetByIdAsync(categoryId).Returns(category);
        _repository.ExistsByNameAsync("Novo", userId, categoryId).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Error.Should().Be(UpdateCategoryError.None);
        result.Category!.Name.Should().Be("Novo");
        result.Category.Color.Should().Be("#FFFFFF");
        await _repository.Received(1).UpdateAsync(category);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Category?)null);

        var result = await _handler.Handle(
            new UpdateCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), "X", "#000000"),
            CancellationToken.None);

        result.Error.Should().Be(UpdateCategoryError.NotFound);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_WhenCategoryBelongsToAnotherUser_ReturnsForbidden()
    {
        var categoryId = Guid.NewGuid();
        var category = CategoryMother.Default(id: categoryId); // userId diferente

        _repository.GetByIdAsync(categoryId).Returns(category);

        var result = await _handler.Handle(
            new UpdateCategoryCommand(categoryId, Guid.NewGuid(), "X", "#000000"),
            CancellationToken.None);

        result.Error.Should().Be(UpdateCategoryError.Forbidden);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_WhenNameConflicts_ReturnsDuplicateName()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = CategoryMother.OwnedBy(userId, categoryId);

        _repository.GetByIdAsync(categoryId).Returns(category);
        _repository.ExistsByNameAsync("Conflito", userId, categoryId).Returns(true);

        var result = await _handler.Handle(
            new UpdateCategoryCommand(categoryId, userId, "Conflito", "#000000"),
            CancellationToken.None);

        result.Error.Should().Be(UpdateCategoryError.DuplicateName);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
    }
}

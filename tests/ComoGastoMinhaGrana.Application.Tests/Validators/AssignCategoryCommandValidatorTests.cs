using ComoGastoMinhaGrana.Application.Commands.AssignCategory;
using FluentValidation.TestHelper;

namespace ComoGastoMinhaGrana.Application.Tests.Validators;

public class AssignCategoryCommandValidatorTests
{
    private readonly AssignCategoryCommandValidator _validator = new();

    [Fact]
    public void TransactionId_WhenEmpty_HasError()
        => _validator.TestValidate(new AssignCategoryCommand(Guid.Empty, Guid.NewGuid(), null))
            .ShouldHaveValidationErrorFor(x => x.TransactionId);

    [Fact]
    public void UserId_WhenEmpty_HasError()
        => _validator.TestValidate(new AssignCategoryCommand(Guid.NewGuid(), Guid.Empty, null))
            .ShouldHaveValidationErrorFor(x => x.UserId);

    [Fact]
    public void Command_WhenBothIdsValid_HasNoErrors()
    {
        var result = _validator.TestValidate(
            new AssignCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), null));

        result.ShouldNotHaveValidationErrorFor(x => x.TransactionId);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}

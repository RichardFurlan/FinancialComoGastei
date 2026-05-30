using ComoGastoMinhaGrana.Application.Commands.CreateCategory;
using FluentValidation.TestHelper;

namespace ComoGastoMinhaGrana.Application.Tests.Validators;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    // --- Name ---

    [Fact]
    public void Name_WhenEmpty_HasError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), "", "#FF5733"))
            .ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Name_WhenExceeds100Chars_HasError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), new string('A', 101), "#FF5733"))
            .ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Name_WhenValid_HasNoError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), "Alimentação", "#FF5733"))
            .ShouldNotHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Name_WhenExactly100Chars_HasNoError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), new string('A', 100), "#FF5733"))
            .ShouldNotHaveValidationErrorFor(x => x.Name);

    // --- Color ---

    [Fact]
    public void Color_WhenEmpty_HasError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), "Lazer", ""))
            .ShouldHaveValidationErrorFor(x => x.Color);

    [Fact]
    public void Color_WhenMissingHash_HasError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), "Lazer", "FF5733"))
            .ShouldHaveValidationErrorFor(x => x.Color);

    [Fact]
    public void Color_WhenThreeDigitHex_HasError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), "Lazer", "#FFF"))
            .ShouldHaveValidationErrorFor(x => x.Color);

    [Fact]
    public void Color_WhenValidLowercase_HasNoError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), "Lazer", "#ff5733"))
            .ShouldNotHaveValidationErrorFor(x => x.Color);

    [Fact]
    public void Color_WhenValidUppercase_HasNoError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.NewGuid(), "Lazer", "#FF5733"))
            .ShouldNotHaveValidationErrorFor(x => x.Color);

    // --- UserId ---

    [Fact]
    public void UserId_WhenEmpty_HasError()
        => _validator.TestValidate(new CreateCategoryCommand(Guid.Empty, "Lazer", "#FF5733"))
            .ShouldHaveValidationErrorFor(x => x.UserId);
}

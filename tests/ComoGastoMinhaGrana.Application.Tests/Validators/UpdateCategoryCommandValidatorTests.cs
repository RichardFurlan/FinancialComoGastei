using ComoGastoMinhaGrana.Application.Commands.UpdateCategory;
using FluentValidation.TestHelper;

namespace ComoGastoMinhaGrana.Application.Tests.Validators;

public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    private static UpdateCategoryCommand Valid() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Moradia", "#00FF00");

    // --- Id ---

    [Fact]
    public void Id_WhenEmpty_HasError()
        => _validator.TestValidate(Valid() with { Id = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.Id);

    // --- Name ---

    [Fact]
    public void Name_WhenEmpty_HasError()
        => _validator.TestValidate(Valid() with { Name = "" })
            .ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Name_WhenExceeds100Chars_HasError()
        => _validator.TestValidate(Valid() with { Name = new string('X', 101) })
            .ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Name_WhenValid_HasNoError()
        => _validator.TestValidate(Valid())
            .ShouldNotHaveValidationErrorFor(x => x.Name);

    // --- Color ---

    [Fact]
    public void Color_WhenMissingHash_HasError()
        => _validator.TestValidate(Valid() with { Color = "00FF00" })
            .ShouldHaveValidationErrorFor(x => x.Color);

    [Fact]
    public void Color_WhenValid_HasNoError()
        => _validator.TestValidate(Valid())
            .ShouldNotHaveValidationErrorFor(x => x.Color);
}

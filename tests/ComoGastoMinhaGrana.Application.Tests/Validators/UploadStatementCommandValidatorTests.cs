using ComoGastoMinhaGrana.Application.Commands.UploadStatement;
using FluentValidation.TestHelper;

namespace ComoGastoMinhaGrana.Application.Tests.Validators;

public class UploadStatementCommandValidatorTests
{
    private readonly UploadStatementCommandValidator _validator = new();

    private static Stream StreamOf(long sizeBytes)
    {
        var ms = new MemoryStream(new byte[sizeBytes]);
        ms.Position = 0;
        return ms;
    }

    // --- UserId ---

    [Fact]
    public void UserId_WhenEmpty_HasError()
        => _validator.TestValidate(new UploadStatementCommand
        {
            UserId = Guid.Empty,
            FileName = "extrato.pdf",
            FileStream = StreamOf(1024)
        }).ShouldHaveValidationErrorFor(x => x.UserId);

    // --- FileName ---

    [Fact]
    public void FileName_WhenEmpty_HasError()
        => _validator.TestValidate(new UploadStatementCommand
        {
            UserId = Guid.NewGuid(),
            FileName = "",
            FileStream = StreamOf(1024)
        }).ShouldHaveValidationErrorFor(x => x.FileName);

    [Theory]
    [InlineData(".exe")]
    [InlineData(".docx")]
    [InlineData(".zip")]
    public void FileName_WhenUnsupportedExtension_HasError(string extension)
        => _validator.TestValidate(new UploadStatementCommand
        {
            UserId = Guid.NewGuid(),
            FileName = $"arquivo{extension}",
            FileStream = StreamOf(1024)
        }).ShouldHaveValidationErrorFor(x => x.FileName);

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".txt")]
    [InlineData(".xlsx")]
    [InlineData(".jpg")]
    public void FileName_WhenSupportedExtension_HasNoError(string extension)
        => _validator.TestValidate(new UploadStatementCommand
        {
            UserId = Guid.NewGuid(),
            FileName = $"extrato{extension}",
            FileStream = StreamOf(1024)
        }).ShouldNotHaveValidationErrorFor(x => x.FileName);

    // --- FileStream ---

    [Fact]
    public void FileStream_WhenEmpty_HasError()
        => _validator.TestValidate(new UploadStatementCommand
        {
            UserId = Guid.NewGuid(),
            FileName = "extrato.pdf",
            FileStream = StreamOf(0)
        }).ShouldHaveValidationErrorFor(x => x.FileStream);

    [Fact]
    public void FileStream_WhenExceeds10MB_HasError()
        => _validator.TestValidate(new UploadStatementCommand
        {
            UserId = Guid.NewGuid(),
            FileName = "extrato.pdf",
            FileStream = StreamOf(11 * 1024 * 1024)
        }).ShouldHaveValidationErrorFor(x => x.FileStream);

    [Fact]
    public void FileStream_WhenExactly10MB_HasNoError()
        => _validator.TestValidate(new UploadStatementCommand
        {
            UserId = Guid.NewGuid(),
            FileName = "extrato.pdf",
            FileStream = StreamOf(10 * 1024 * 1024)
        }).ShouldNotHaveValidationErrorFor(x => x.FileStream);
}

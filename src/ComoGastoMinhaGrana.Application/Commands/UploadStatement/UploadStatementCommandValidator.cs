using FluentValidation;

namespace ComoGastoMinhaGrana.Application.Commands.UploadStatement;

public class UploadStatementCommandValidator : AbstractValidator<UploadStatementCommand>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".txt", ".xlsx", ".xls", ".jpg", ".jpeg", ".png"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public UploadStatementCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId é obrigatório.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Nome do arquivo é obrigatório.")
            .Must(name => AllowedExtensions.Contains(Path.GetExtension(name)))
            .WithMessage($"Formato não suportado. Formatos aceitos: {string.Join(", ", AllowedExtensions)}");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("Arquivo é obrigatório.")
            .Must(s => s.Length > 0).WithMessage("Arquivo está vazio.")
            .Must(s => s.Length <= MaxFileSizeBytes).WithMessage("Arquivo excede o tamanho máximo de 10 MB.");
    }
}

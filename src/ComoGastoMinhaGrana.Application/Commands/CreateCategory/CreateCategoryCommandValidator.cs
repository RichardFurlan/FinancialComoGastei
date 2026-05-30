using FluentValidation;

namespace ComoGastoMinhaGrana.Application.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome não pode ultrapassar 100 caracteres.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Cor é obrigatória.")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Cor deve ser um hex válido (ex: #FF5733).");
    }
}

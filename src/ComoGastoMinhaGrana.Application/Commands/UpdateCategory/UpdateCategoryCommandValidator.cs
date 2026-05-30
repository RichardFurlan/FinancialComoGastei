using FluentValidation;

namespace ComoGastoMinhaGrana.Application.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome não pode ultrapassar 100 caracteres.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Cor é obrigatória.")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Cor deve ser um hex válido (ex: #FF5733).");
    }
}

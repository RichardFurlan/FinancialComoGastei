using FluentValidation;

namespace ComoGastoMinhaGrana.Application.Commands.CreateCategoryRule;

public class CreateCategoryRuleCommandValidator : AbstractValidator<CreateCategoryRuleCommand>
{
    public CreateCategoryRuleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Termo de busca é obrigatório.")
            .MaximumLength(200).WithMessage("Termo de busca não pode ultrapassar 200 caracteres.");
        RuleFor(x => x.RuleMatchType)
            .IsInEnum().WithMessage("Tipo de correspondência inválido.");
    }
}

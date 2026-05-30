using FluentValidation;

namespace ComoGastoMinhaGrana.Application.Commands.AssignCategory;

public class AssignCategoryCommandValidator : AbstractValidator<AssignCategoryCommand>
{
    public AssignCategoryCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

using FluentValidation;

namespace ComoGastoMinhaGrana.Application.Commands.CreateReport;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do relatório é obrigatório.")
            .MaximumLength(100);
        RuleFor(x => x.StatementIds)
            .NotEmpty().WithMessage("Selecione pelo menos 1 extrato.")
            .Must(ids => ids.Count <= 6).WithMessage("Máximo de 6 extratos por relatório.");
    }
}

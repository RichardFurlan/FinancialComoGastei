using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Services;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.ApplyRulesToStatement;

public class ApplyRulesToStatementCommandHandler : IRequestHandler<ApplyRulesToStatementCommand, ApplyRulesResult>
{
    private readonly IFinancialStatementRepository _statementRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRuleRepository _ruleRepository;
    private readonly CategoryRuleApplierService _applier;

    public ApplyRulesToStatementCommandHandler(
        IFinancialStatementRepository statementRepository,
        ITransactionRepository transactionRepository,
        ICategoryRuleRepository ruleRepository,
        CategoryRuleApplierService applier)
    {
        _statementRepository = statementRepository;
        _transactionRepository = transactionRepository;
        _ruleRepository = ruleRepository;
        _applier = applier;
    }

    public async Task<ApplyRulesResult> Handle(ApplyRulesToStatementCommand request, CancellationToken cancellationToken)
    {
        var statement = await _statementRepository.GetByIdAndUserIdAsync(request.StatementId, request.UserId);
        if (statement is null)
            return new ApplyRulesResult(0, ApplyRulesError.NotFound);

        if (statement.UserId != request.UserId)
            return new ApplyRulesResult(0, ApplyRulesError.Forbidden);

        var transactions = (await _transactionRepository.GetByStatementIdAsync(request.StatementId)).ToList();
        var rules = (await _ruleRepository.GetByUserIdAsync(request.UserId)).ToList();

        var changed = _applier.Apply(transactions, rules);

        if (changed.Count > 0)
            await _transactionRepository.UpdateRangeAsync(changed);

        return new ApplyRulesResult(changed.Count);
    }
}

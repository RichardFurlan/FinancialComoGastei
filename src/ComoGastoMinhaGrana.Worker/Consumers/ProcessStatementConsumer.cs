using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Common.Messages;
using ComoGastoMinhaGrana.Application.Services;
using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Domain.Enums;
using ComoGastoMinhaGrana.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ComoGastoMinhaGrana.Worker.Consumers;

public class ProcessStatementConsumer : IConsumer<ProcessStatementMessage>
{
    private readonly IFinancialStatementRepository _statementRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRuleRepository _ruleRepository;
    private readonly IAIService _aiService;
    private readonly CategoryRuleApplierService _ruleApplier;
    private readonly ILogger<ProcessStatementConsumer> _logger;

    public ProcessStatementConsumer(
        IFinancialStatementRepository statementRepository,
        ITransactionRepository transactionRepository,
        ICategoryRuleRepository ruleRepository,
        IAIService aiService,
        CategoryRuleApplierService ruleApplier,
        ILogger<ProcessStatementConsumer> logger)
    {
        _statementRepository = statementRepository;
        _transactionRepository = transactionRepository;
        _ruleRepository = ruleRepository;
        _aiService = aiService;
        _ruleApplier = ruleApplier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessStatementMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processando extrato {StatementId} para usuário {UserId}", msg.StatementId, msg.UserId);

        var statement = await _statementRepository.GetByIdAsync(msg.StatementId);
        if (statement is null)
        {
            _logger.LogWarning("Extrato {StatementId} não encontrado.", msg.StatementId);
            return;
        }

        statement.Status = StatementStatus.Processing;
        await _statementRepository.UpdateAsync(statement);

        try
        {
            // 1. Extrair transações via DeepSeek (texto já sanitizado)
            var structuredTransactions = await _aiService.ExtractTransactionsAsync(msg.SanitizedText);

            if (structuredTransactions.Count == 0)
            {
                statement.Status = StatementStatus.Error;
                statement.ErrorMessage = "Nenhuma transação pôde ser extraída do arquivo.";
                await _statementRepository.UpdateAsync(statement);
                return;
            }

            // 2. Persistir transações
            var transactions = structuredTransactions.Select(t => new Transaction
            {
                Id = Guid.NewGuid(),
                Date = t.Date,
                OriginalDescription = t.Description,
                Amount = t.Amount,
                Currency = string.IsNullOrEmpty(t.Currency) ? "BRL" : t.Currency,
                FinancialStatementId = statement.Id
            }).ToList();

            statement.BaseCurrency = transactions.First().Currency;
            await _transactionRepository.AddRangeAsync(transactions);

            // 3. Aplicar Regras de Ouro (carregadas uma única vez, aplicação em memória)
            var rules = (await _ruleRepository.GetByUserIdAsync(msg.UserId)).ToList();
            if (rules.Count > 0)
            {
                var changed = _ruleApplier.Apply(transactions, rules);
                if (changed.Count > 0)
                {
                    await _transactionRepository.UpdateRangeAsync(changed);
                    _logger.LogInformation("{Count} transações categorizadas pelas Regras de Ouro.", changed.Count);
                }
            }

            // 4. Montar resumo e gerar análise
            var summary = AnalysisPromptBuilder.Build(structuredTransactions);
            var analysisMarkdown = await _aiService.GenerateAnalysisAsync(summary);

            // 4. Salvar análise
            var analysis = new FinancialAnalysis
            {
                Id = Guid.NewGuid(),
                FinancialStatementId = statement.Id,
                MarkdownContent = analysisMarkdown,
                GeneratedAt = DateTime.UtcNow
            };

            statement.Status = StatementStatus.Processed;
            await _statementRepository.UpdateAsync(statement);

            // Salvar análise diretamente no contexto (via repositório de statement já inclui o relacionamento)
            // Usamos o DbContext via repositório não exposto — adicionamos Analysis diretamente
            statement.Analysis = analysis;
            await _statementRepository.UpdateAsync(statement);

            _logger.LogInformation("Extrato {StatementId} processado: {Count} transações.", msg.StatementId, transactions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar extrato {StatementId}.", msg.StatementId);
            statement.Status = StatementStatus.Error;
            statement.ErrorMessage = "Falha inesperada ao processar o arquivo. Tente novamente.";
            await _statementRepository.UpdateAsync(statement);
        }
    }
}

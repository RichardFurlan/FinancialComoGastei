using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Common.Messages;
using ComoGastoMinhaGrana.Application.Services;
using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Domain.Enums;
using ComoGastoMinhaGrana.Infrastructure.Persistence;
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
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProcessStatementConsumer> _logger;

    public ProcessStatementConsumer(
        IFinancialStatementRepository statementRepository,
        ITransactionRepository transactionRepository,
        ICategoryRuleRepository ruleRepository,
        IAIService aiService,
        CategoryRuleApplierService ruleApplier,
        ApplicationDbContext context,
        ILogger<ProcessStatementConsumer> logger)
    {
        _statementRepository = statementRepository;
        _transactionRepository = transactionRepository;
        _ruleRepository = ruleRepository;
        _aiService = aiService;
        _ruleApplier = ruleApplier;
        _context = context;
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

        // Idempotência: não reprocessar extratos já finalizados
        if (statement.Status is StatementStatus.Processed or StatementStatus.Error)
        {
            _logger.LogInformation("Extrato {StatementId} já finalizado (status: {Status}), ignorando.", msg.StatementId, statement.Status);
            return;
        }

        // Marca como Processing fora da transação para que outros workers saibam que está em andamento
        statement.Status = StatementStatus.Processing;
        await _statementRepository.UpdateAsync(statement);

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Extrair transações via IA (texto já sanitizado pela API)
            var structuredTransactions = await _aiService.ExtractTransactionsAsync(msg.SanitizedText);

            if (structuredTransactions.Count == 0)
            {
                statement.Status = StatementStatus.Error;
                statement.ErrorMessage = "Nenhuma transação pôde ser extraída do arquivo.";
                await _statementRepository.UpdateAsync(statement);
                await tx.CommitAsync();
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

            // 3. Aplicar Regras de Ouro
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

            // 4. Gerar análise financeira
            var summary = AnalysisPromptBuilder.Build(structuredTransactions);
            var analysisMarkdown = await _aiService.GenerateAnalysisAsync(summary);

            var analysis = new FinancialAnalysis
            {
                Id = Guid.NewGuid(),
                FinancialStatementId = statement.Id,
                MarkdownContent = analysisMarkdown,
                GeneratedAt = DateTime.UtcNow
            };

            // 5. Persistir análise e marcar como concluído — tudo no mesmo commit
            await _context.FinancialAnalyses.AddAsync(analysis);
            statement.Status = StatementStatus.Processed;
            await _statementRepository.UpdateAsync(statement);

            await tx.CommitAsync();
            _logger.LogInformation("Extrato {StatementId} processado: {Count} transações.", msg.StatementId, transactions.Count);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Erro ao processar extrato {StatementId}.", msg.StatementId);

            // Recarrega o statement após rollback para evitar estado obsoleto do EF Core
            _context.ChangeTracker.Clear();
            var failedStatement = await _statementRepository.GetByIdAsync(msg.StatementId);
            if (failedStatement is not null)
            {
                failedStatement.Status = StatementStatus.Error;
                failedStatement.ErrorMessage = "Falha inesperada ao processar o arquivo. Tente novamente.";
                await _statementRepository.UpdateAsync(failedStatement);
            }
        }
    }
}

using ComoGastoMinhaGrana.Application.Common.Models;

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IAIService
{
    Task<List<StructuredTransaction>> ExtractTransactionsAsync(string sanitizedText);
    Task<string> GenerateAnalysisAsync(string transactionSummary);
}

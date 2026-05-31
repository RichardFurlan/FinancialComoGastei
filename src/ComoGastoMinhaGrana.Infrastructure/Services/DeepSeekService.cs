using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComoGastoMinhaGrana.Infrastructure.Services;

public class DeepSeekService : IAIService
{
    private readonly HttpClient _http;
    private readonly ILogger<DeepSeekService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    private const string BaseUrl = "https://openrouter.ai/api/v1/chat/completions";

    public DeepSeekService(HttpClient http, IConfiguration config, ILogger<DeepSeekService> logger)
    {
        _http = http;
        _logger = logger;
        _apiKey = config["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey não configurada.");
        _model = config["OpenRouter:Model"] ?? "deepseek/deepseek-chat";
    }

    public async Task<List<StructuredTransaction>> ExtractTransactionsAsync(string sanitizedText)
    {
        var systemPrompt = "Você é um assistente financeiro especialista em extrair dados de extratos bancários. " +
                           "Responda APENAS com um array JSON sem explicações. " +
                           "Cada objeto deve ter: Date (ISO 8601), Description (string), Amount (decimal, positivo para crédito, negativo para débito), Currency (string, ex: BRL). " +
                           "Exemplo: [{\"Date\":\"2024-01-15\",\"Description\":\"MERCADO LIVRE\",\"Amount\":-150.00,\"Currency\":\"BRL\"}]";

        var userPrompt = $"Extraia todas as transações do seguinte extrato financeiro:\n\n{sanitizedText}";

        var responseText = await CallApiAsync(systemPrompt, userPrompt);

        try
        {
            var json = ExtractJsonArray(responseText);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<StructuredTransaction>>(json, options) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao desserializar transações da resposta do DeepSeek.");
            return new();
        }
    }

    public async Task<string> GenerateAnalysisAsync(string transactionSummary)
    {
        var systemPrompt = "Você é um consultor financeiro pessoal empático e direto. " +
                           "Analise os dados financeiros fornecidos e responda em português do Brasil em formato Markdown estruturado. " +
                           "Use exatamente estas seções na ordem abaixo:\n\n" +
                           "## Resumo Geral\n" +
                           "## Maiores Gastos\n" +
                           "## Padrão de Comportamento\n" +
                           "## Alertas\n" +
                           "## Sugestão de Ação\n\n" +
                           "Seja específico com valores e percentuais quando possível. Não invente dados que não estejam no resumo.";

        return await CallApiAsync(systemPrompt, transactionSummary);
    }

    private async Task<string> CallApiAsync(string systemPrompt, string userPrompt)
    {
        var request = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        httpRequest.Headers.Add("HTTP-Referer", "https://comogastominhagrana.app");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var response = await _http.SendAsync(httpRequest, cts.Token);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>();
        return result?.Choices?.FirstOrDefault()?.Message?.Content
               ?? throw new InvalidOperationException("Resposta vazia do OpenRouter.");
    }

    private static string ExtractJsonArray(string text)
    {
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');
        if (start >= 0 && end > start)
            return text[start..(end + 1)];
        return text;
    }

    private record OpenRouterResponse(
        [property: JsonPropertyName("choices")] List<OpenRouterChoice>? Choices);

    private record OpenRouterChoice(
        [property: JsonPropertyName("message")] OpenRouterMessage? Message);

    private record OpenRouterMessage(
        [property: JsonPropertyName("content")] string? Content);
}

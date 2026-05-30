namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IPdfExtractionService
{
    Task<string> ExtractTextAsync(Stream pdfStream);
}

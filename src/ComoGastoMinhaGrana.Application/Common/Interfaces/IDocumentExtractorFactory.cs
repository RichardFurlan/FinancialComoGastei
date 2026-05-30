namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IDocumentExtractorFactory
{
    bool CanHandle(string fileName);
    Task<string> ExtractTextAsync(Stream stream, string fileName);
}

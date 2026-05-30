namespace ComoGastoMinhaGrana.Infrastructure.Services.Extraction;

internal interface IDocumentTextExtractor
{
    bool CanHandle(string fileExtension);
    Task<string> ExtractTextAsync(Stream stream, string fileName);
}

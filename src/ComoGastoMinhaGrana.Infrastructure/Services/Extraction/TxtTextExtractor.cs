namespace ComoGastoMinhaGrana.Infrastructure.Services.Extraction;

internal class TxtTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}

using ComoGastoMinhaGrana.Application.Common.Interfaces;

namespace ComoGastoMinhaGrana.Infrastructure.Services.Extraction;

internal class DocumentExtractorFactory : IDocumentExtractorFactory
{
    private readonly IEnumerable<IDocumentTextExtractor> _extractors;

    public DocumentExtractorFactory(IEnumerable<IDocumentTextExtractor> extractors)
    {
        _extractors = extractors;
    }

    public bool CanHandle(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return _extractors.Any(e => e.CanHandle(ext));
    }

    public Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        var ext = Path.GetExtension(fileName);
        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(ext))
            ?? throw new NotSupportedException($"Formato '{ext}' não é suportado.");

        return extractor.ExtractTextAsync(stream, fileName);
    }
}

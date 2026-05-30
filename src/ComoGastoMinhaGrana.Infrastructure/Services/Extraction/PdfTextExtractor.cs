using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ComoGastoMinhaGrana.Infrastructure.Services.Extraction;

internal class PdfTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        using var document = PdfDocument.Open(stream);
        var lines = document.GetPages()
            .SelectMany(p => p.GetWords())
            .Select(w => w.Text);

        return Task.FromResult(string.Join(" ", lines));
    }
}

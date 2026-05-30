namespace ComoGastoMinhaGrana.Infrastructure.Services.Extraction;

internal class ImageTextExtractor : IDocumentTextExtractor
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    public bool CanHandle(string fileExtension) => SupportedExtensions.Contains(fileExtension);

    public Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        // OCR via Tesseract será implementado quando o microservice Go/Rust for criado.
        // Por ora retorna erro claro para o usuário.
        throw new NotSupportedException(
            "Extração de texto de imagens ainda não está disponível. " +
            "Utilize PDF, TXT ou Excel por enquanto.");
    }
}

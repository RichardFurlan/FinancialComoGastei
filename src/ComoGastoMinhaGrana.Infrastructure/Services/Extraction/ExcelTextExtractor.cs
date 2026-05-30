using ClosedXML.Excel;

namespace ComoGastoMinhaGrana.Infrastructure.Services.Extraction;

internal class ExcelTextExtractor : IDocumentTextExtractor
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx", ".xls"
    };

    public bool CanHandle(string fileExtension) => SupportedExtensions.Contains(fileExtension);

    public Task<string> ExtractTextAsync(Stream stream, string fileName)
    {
        using var workbook = new XLWorkbook(stream);
        var sb = new System.Text.StringBuilder();

        foreach (var worksheet in workbook.Worksheets)
        {
            foreach (var row in worksheet.RowsUsed())
            {
                var cells = row.CellsUsed().Select(c => c.GetValue<string>());
                sb.AppendLine(string.Join("\t", cells));
            }
        }

        return Task.FromResult(sb.ToString());
    }
}

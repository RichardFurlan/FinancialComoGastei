using System.Text;
using ClosedXML.Excel;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ComoGastoMinhaGrana.Infrastructure.Services;

public class ExportService : IExportService
{
    public byte[] ToCsv(FinancialStatement statement)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Data,Descrição,Categoria,Moeda,Valor");

        foreach (var t in statement.Transactions.OrderByDescending(t => t.Date))
        {
            var line = string.Join(",",
                t.Date.ToString("dd/MM/yyyy"),
                EscapeCsv(t.OriginalDescription),
                EscapeCsv(t.Category?.Name ?? ""),
                t.Currency,
                t.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine(line);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ToXlsx(FinancialStatement statement)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Transações");

        // Cabeçalho
        ws.Cell(1, 1).Value = "Data";
        ws.Cell(1, 2).Value = "Descrição";
        ws.Cell(1, 3).Value = "Categoria";
        ws.Cell(1, 4).Value = "Moeda";
        ws.Cell(1, 5).Value = "Valor";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Dados
        var transactions = statement.Transactions.OrderByDescending(t => t.Date).ToList();
        for (int i = 0; i < transactions.Count; i++)
        {
            var t = transactions[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = t.Date.ToString("dd/MM/yyyy");
            ws.Cell(row, 2).Value = t.OriginalDescription;
            ws.Cell(row, 3).Value = t.Category?.Name ?? "";
            ws.Cell(row, 4).Value = t.Currency;
            ws.Cell(row, 5).Value = t.Amount;

            if (t.Amount < 0)
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
            else
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ToPdf(FinancialStatement statement)
    {
        var transactions = statement.Transactions.OrderByDescending(t => t.Date).ToList();
        var totalDebits = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
        var totalCredits = transactions.Where(t => t.Amount >= 0).Sum(t => t.Amount);
        var balance = totalCredits - totalDebits;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(statement.FileName).FontSize(14).Bold();
                    col.Item().Text($"Upload: {statement.UploadDate:dd/MM/yyyy}  ·  Moeda: {statement.BaseCurrency}  ·  {transactions.Count} transações")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(70);  // Data
                        cols.RelativeColumn(3);    // Descrição
                        cols.RelativeColumn(1.5f); // Categoria
                        cols.ConstantColumn(50);   // Moeda
                        cols.ConstantColumn(80);   // Valor
                    });

                    // Cabeçalho
                    static IContainer HeaderCell(IContainer c) =>
                        c.Background(Colors.Grey.Lighten3).Padding(4).AlignMiddle();

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Data").Bold();
                        h.Cell().Element(HeaderCell).Text("Descrição").Bold();
                        h.Cell().Element(HeaderCell).Text("Categoria").Bold();
                        h.Cell().Element(HeaderCell).AlignCenter().Text("Moeda").Bold();
                        h.Cell().Element(HeaderCell).AlignRight().Text("Valor").Bold();
                    });

                    // Linhas
                    foreach (var t in transactions)
                    {
                        var color = t.Amount < 0 ? Colors.Red.Darken2 : Colors.Green.Darken2;

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4)
                            .Text(t.Date.ToString("dd/MM/yy")).FontSize(9);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4)
                            .Text(t.OriginalDescription).FontSize(9);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4)
                            .Text(t.Category?.Name ?? "—").FontSize(9).FontColor(Colors.Grey.Medium);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4)
                            .AlignCenter().Text(t.Currency).FontSize(9);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4)
                            .AlignRight().Text($"{t.Amount:F2}").FontSize(9).FontColor(color);
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text($"Saídas: {totalDebits:F2} {statement.BaseCurrency}")
                            .FontSize(9).FontColor(Colors.Red.Darken2);
                        row.RelativeItem().AlignCenter()
                            .Text($"Entradas: {totalCredits:F2} {statement.BaseCurrency}")
                            .FontSize(9).FontColor(Colors.Green.Darken2);
                        row.RelativeItem().AlignRight()
                            .Text($"Saldo: {balance:F2} {statement.BaseCurrency}")
                            .FontSize(9).Bold();
                    });
                });
            });
        }).GeneratePdf();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}

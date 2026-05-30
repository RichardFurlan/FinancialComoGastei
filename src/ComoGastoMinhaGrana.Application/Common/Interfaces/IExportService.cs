using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Common.Interfaces;

public interface IExportService
{
    byte[] ToCsv(FinancialStatement statement);
    byte[] ToXlsx(FinancialStatement statement);
    byte[] ToPdf(FinancialStatement statement);
}

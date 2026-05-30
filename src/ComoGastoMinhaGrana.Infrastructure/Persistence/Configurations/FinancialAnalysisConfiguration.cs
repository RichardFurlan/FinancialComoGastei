using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Configurations;

public class FinancialAnalysisConfiguration : IEntityTypeConfiguration<FinancialAnalysis>
{
    public void Configure(EntityTypeBuilder<FinancialAnalysis> builder)
    {
        builder.ToTable("FinancialAnalyses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.MarkdownContent)
            .IsRequired();
    }
}

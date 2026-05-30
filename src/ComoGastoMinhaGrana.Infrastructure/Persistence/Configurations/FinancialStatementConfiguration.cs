using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Configurations;

public class FinancialStatementConfiguration : IEntityTypeConfiguration<FinancialStatement>
{
    public void Configure(EntityTypeBuilder<FinancialStatement> builder)
    {
        builder.ToTable("FinancialStatements");

        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(fs => fs.FileExtension)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(fs => fs.BaseCurrency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(fs => fs.Status)
            .HasConversion<string>();

        builder.Property(fs => fs.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasOne(fs => fs.User)
            .WithMany(u => u.FinancialStatements)
            .HasForeignKey(fs => fs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(fs => fs.Transactions)
            .WithOne(t => t.FinancialStatement)
            .HasForeignKey(t => t.FinancialStatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fs => fs.Analysis)
            .WithOne(a => a.FinancialStatement)
            .HasForeignKey<FinancialAnalysis>(a => a.FinancialStatementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

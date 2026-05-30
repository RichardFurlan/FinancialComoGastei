using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.UserId).IsRequired();

        builder.HasMany(r => r.Statements)
            .WithOne(rs => rs.Report)
            .HasForeignKey(rs => rs.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
    }
}

public class ReportStatementConfiguration : IEntityTypeConfiguration<ReportStatement>
{
    public void Configure(EntityTypeBuilder<ReportStatement> builder)
    {
        builder.ToTable("ReportStatements");
        builder.HasKey(rs => new { rs.ReportId, rs.StatementId });

        builder.HasOne(rs => rs.Statement)
            .WithMany()
            .HasForeignKey(rs => rs.StatementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

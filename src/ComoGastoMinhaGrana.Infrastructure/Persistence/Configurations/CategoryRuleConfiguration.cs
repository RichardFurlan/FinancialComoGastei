using ComoGastoMinhaGrana.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComoGastoMinhaGrana.Infrastructure.Persistence.Configurations;

public class CategoryRuleConfiguration : IEntityTypeConfiguration<CategoryRule>
{
    public void Configure(EntityTypeBuilder<CategoryRule> builder)
    {
        builder.ToTable("CategoryRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.SearchTerm)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.RuleMatchType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
    }
}

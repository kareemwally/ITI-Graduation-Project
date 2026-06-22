using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class AIVerificationResultConfiguration : IEntityTypeConfiguration<AIVerificationResult>
    {
        public void Configure(EntityTypeBuilder<AIVerificationResult> builder)
        {
            builder.ToTable("AIVerificationResults");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.ExtractedFields).IsRequired(); // JSON
            builder.Property(a => a.ConfidenceScore).HasPrecision(4, 3).IsRequired();
            builder.Property(a => a.Mismatches); // JSON, nullable
            builder.Property(a => a.AIRecommendation)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(a => a.ModelVersion).IsRequired().HasMaxLength(50);
            builder.Property(a => a.CreatedAt).IsRequired();

            // 1..1 with VerificationCase, enforced by the unique key.
            builder.HasIndex(a => a.VerificationCaseId).IsUnique();
            builder.HasOne(a => a.VerificationCase)
                   .WithOne(v => v.AIVerificationResult)
                   .HasForeignKey<AIVerificationResult>(a => a.VerificationCaseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class AISearchLogConfiguration : IEntityTypeConfiguration<AISearchLog>
    {
        public void Configure(EntityTypeBuilder<AISearchLog> builder)
        {
            builder.ToTable("AISearchLogs");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.PromptText).IsRequired();
            builder.Property(a => a.ExtractedFilters).IsRequired(); // JSON
            builder.Property(a => a.ResultsCount).IsRequired();
            builder.Property(a => a.TopListingIds).IsRequired(); // JSON
            builder.Property(a => a.ModelVersion).IsRequired().HasMaxLength(50);
            builder.Property(a => a.CreatedAt).IsRequired();

            builder.HasOne(a => a.User)
                   .WithMany(u => u.AISearchLogs)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

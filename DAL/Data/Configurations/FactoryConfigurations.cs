using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class FactoryConfiguration : IEntityTypeConfiguration<Factory>
    {
        public void Configure(EntityTypeBuilder<Factory> builder)
        {
            builder.ToTable("Factories");
            builder.HasKey(f => f.Id);

            builder.Property(f => f.LegalName).IsRequired().HasMaxLength(255);
            builder.Property(f => f.CommercialRegistryNo).IsRequired().HasMaxLength(50);
            builder.Property(f => f.TaxCardNo).IsRequired().HasMaxLength(50);
            builder.Property(f => f.Address).IsRequired().HasMaxLength(500);
            builder.Property(f => f.Sector).IsRequired().HasMaxLength(100);
            builder.Property(f => f.LogoUrl).HasMaxLength(500);
            builder.Property(f => f.VerificationStatus)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasIndex(f => f.CommercialRegistryNo).IsUnique();
            builder.HasIndex(f => f.TaxCardNo).IsUnique();
            builder.HasIndex(f => f.UserId).IsUnique(); // 1..1 owner for the MVP

            builder.HasOne(f => f.Owner)
                   .WithOne(u => u.Factory)
                   .HasForeignKey<Factory>(f => f.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Governorate)
                   .WithMany(g => g.Factories)
                   .HasForeignKey(f => f.GovernorateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.City)
                   .WithMany(c => c.Factories)
                   .HasForeignKey(f => f.CityId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(f => !f.IsDeleted);
        }
    }

    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents", t =>
                t.HasCheckConstraint("CK_Documents_FactoryOrOrder",
                    "[FactoryId] IS NOT NULL OR [OrderId] IS NOT NULL"));

            builder.HasKey(d => d.Id);

            builder.Property(d => d.DocumentType).IsRequired().HasMaxLength(50);
            builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(500);
            builder.Property(d => d.UploadedAt).IsRequired();

            builder.HasOne(d => d.Factory)
                   .WithMany(f => f.Documents)
                   .HasForeignKey(d => d.FactoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Order)
                   .WithMany(o => o.Documents)
                   .HasForeignKey(d => d.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class VerificationCaseConfiguration : IEntityTypeConfiguration<VerificationCase>
    {
        public void Configure(EntityTypeBuilder<VerificationCase> builder)
        {
            builder.ToTable("VerificationCases");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Status)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(v => v.Decision)
                   .HasConversion<string>().HasMaxLength(20);
            builder.Property(v => v.Notes);
            builder.Property(v => v.CreatedAt).IsRequired();

            builder.HasIndex(v => new { v.FactoryId, v.CreatedAt });

            builder.HasOne(v => v.Factory)
                   .WithMany(f => f.VerificationCases)
                   .HasForeignKey(v => v.FactoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.Reviewer)
                   .WithMany(u => u.ReviewedVerificationCases)
                   .HasForeignKey(v => v.ReviewerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

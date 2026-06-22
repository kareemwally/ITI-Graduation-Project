using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

            builder.HasOne(c => c.Parent)
                   .WithMany(c => c.Children)
                   .HasForeignKey(c => c.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Seed a small category tree (M003_FactoriesAndCategories).
            builder.HasData(
                new Category { Id = 1, ParentId = null, Name = "Metals" },
                new Category { Id = 2, ParentId = null, Name = "Plastics" },
                new Category { Id = 3, ParentId = null, Name = "Textiles" },
                new Category { Id = 4, ParentId = null, Name = "Chemicals" },
                new Category { Id = 5, ParentId = 1, Name = "Steel Scrap" },
                new Category { Id = 6, ParentId = 1, Name = "Aluminium Scrap" },
                new Category { Id = 7, ParentId = 2, Name = "PET" },
                new Category { Id = 8, ParentId = 2, Name = "HDPE" }
            );
        }
    }

    public class ListingConfiguration : IEntityTypeConfiguration<Listing>
    {
        public void Configure(EntityTypeBuilder<Listing> builder)
        {
            builder.ToTable("Listings");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Title).IsRequired().HasMaxLength(200);
            builder.Property(l => l.Description).IsRequired();
            builder.Property(l => l.MaterialType).IsRequired().HasMaxLength(100);
            builder.Property(l => l.MaterialCondition)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(l => l.Quantity).HasPrecision(18, 3).IsRequired();
            builder.Property(l => l.MeasureUnit).IsRequired().HasMaxLength(20);
            builder.Property(l => l.Price).HasPrecision(18, 2).IsRequired();
            builder.Property(l => l.MinOrderQuantity).HasPrecision(18, 3).IsRequired();
            builder.Property(l => l.DeliveryType)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(l => l.PreferPayMethod)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(l => l.CustomCatName).HasMaxLength(100);
            builder.Property(l => l.Status)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(l => l.CreatedAt).IsRequired();
            builder.Property(l => l.ExpiryDate).IsRequired();

            // Composite search index + recency index.
            builder.HasIndex(l => new { l.MaterialType, l.Status, l.CategoryId });
            builder.HasIndex(l => l.CreatedAt);

            builder.HasOne(l => l.Factory)
                   .WithMany(f => f.Listings)
                   .HasForeignKey(l => l.FactoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Category)
                   .WithMany(c => c.Listings)
                   .HasForeignKey(l => l.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(l => !l.IsDeleted);
        }
    }

    public class ListingMediaConfiguration : IEntityTypeConfiguration<ListingMedia>
    {
        public void Configure(EntityTypeBuilder<ListingMedia> builder)
        {
            builder.ToTable("ListingMedia");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.MediaUrl).IsRequired().HasMaxLength(500);
            builder.Property(m => m.MediaType)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasOne(m => m.Listing)
                   .WithMany(l => l.Media)
                   .HasForeignKey(m => m.ListingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SavedListingConfiguration : IEntityTypeConfiguration<SavedListing>
    {
        public void Configure(EntityTypeBuilder<SavedListing> builder)
        {
            builder.ToTable("SavedListings");
            builder.HasKey(s => new { s.UserId, s.ListingId });

            builder.Property(s => s.SavedAt).IsRequired();

            builder.HasOne(s => s.User)
                   .WithMany(u => u.SavedListings)
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Listing)
                   .WithMany(l => l.SavedByUsers)
                   .HasForeignKey(s => s.ListingId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

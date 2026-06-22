using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.ToTable("Governorates");
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(g => g.Name).IsUnique();

            // Seed the 27 Egyptian governorates (M002_Geography).
            var names = new[]
            {
                "Cairo", "Giza", "Alexandria", "Dakahlia", "Red Sea", "Beheira", "Fayoum",
                "Gharbia", "Ismailia", "Menofia", "Minya", "Qalyubia", "New Valley", "Suez",
                "Aswan", "Assiut", "Beni Suef", "Port Said", "Damietta", "Sharqia",
                "South Sinai", "Kafr El Sheikh", "Matrouh", "Luxor", "Qena", "North Sinai", "Sohag"
            };
            builder.HasData(names.Select((n, i) => new Governorate { Id = i + 1, Name = n }));
        }
    }

    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("Cities");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(c => new { c.GovernorateId, c.Name });

            builder.HasOne(c => c.Governorate)
                   .WithMany(g => g.Cities)
                   .HasForeignKey(c => c.GovernorateId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

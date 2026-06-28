using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
            builder.Property(u => u.NationalId).HasMaxLength(20);
            builder.Property(u => u.VerificationStatus)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(u => u.CreatedAt).IsRequired();

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }

}
using DAL.Models;
using DAL.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            builder.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.Property(u => u.NationalId).HasMaxLength(20);
            builder.Property(u => u.VerificationStatus)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(u => u.CreatedAt).IsRequired();

            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
            builder.HasIndex(r => r.RoleName).IsUnique();

            // Seed the baseline roles (M001_AuthAndRoles).
            builder.HasData(
                new Role { Id = 1, RoleName = "FactoryOwner" },
                new Role { Id = 2, RoleName = "VerificationOfficer" },
                new Role { Id = 3, RoleName = "FinanceAdmin" },
                new Role { Id = 4, RoleName = "SupportAgent" },
                new Role { Id = 5, RoleName = "SuperAdmin" }
            );
        }
    }

    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

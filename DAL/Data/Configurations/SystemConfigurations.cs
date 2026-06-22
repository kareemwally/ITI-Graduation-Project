using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Message).IsRequired();
            builder.Property(n => n.Type).IsRequired().HasMaxLength(50);
            builder.Property(n => n.RelatedLink).HasMaxLength(500);
            builder.Property(n => n.CreatedAt).IsRequired();

            builder.HasIndex(n => new { n.UserId, n.IsRead });

            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
            builder.Property(a => a.TargetEntity).IsRequired().HasMaxLength(50);
            builder.Property(a => a.TargetId).IsRequired();
            builder.Property(a => a.Payload).IsRequired(); // JSON snapshot
            builder.Property(a => a.CreatedAt).IsRequired();

            builder.HasIndex(a => new { a.TargetEntity, a.TargetId });

            builder.HasOne(a => a.Admin)
                   .WithMany(u => u.AuditLogs)
                   .HasForeignKey(a => a.AdminId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

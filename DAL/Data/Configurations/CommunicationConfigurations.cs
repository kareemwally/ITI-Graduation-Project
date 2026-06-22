using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("Chats");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Status)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(c => c.StartedAt).IsRequired();

            // Prevent duplicate threads for the same (listing, buyer).
            builder.HasIndex(c => new { c.ListingId, c.BuyerId }).IsUnique();

            builder.HasOne(c => c.Listing)
                   .WithMany(l => l.Chats)
                   .HasForeignKey(c => c.ListingId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Buyer)
                   .WithMany(u => u.BuyerChats)
                   .HasForeignKey(c => c.BuyerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Seller)
                   .WithMany(u => u.SellerChats)
                   .HasForeignKey(c => c.SellerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Messages");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Content);
            builder.Property(m => m.MessageType)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(m => m.ActionUrl).HasMaxLength(500);
            builder.Property(m => m.AttachmentUrl).HasMaxLength(500);
            builder.Property(m => m.SentAt).IsRequired();

            builder.HasIndex(m => m.ChatId);

            builder.HasOne(m => m.Chat)
                   .WithMany(c => c.Messages)
                   .HasForeignKey(m => m.ChatId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Sender)
                   .WithMany(u => u.SentMessages)
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

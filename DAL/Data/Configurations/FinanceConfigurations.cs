using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);

            // حقول الكميات والأسعار والعمولة الموحدة
            builder.Property(o => o.AgreedQuantity).HasPrecision(18, 3).IsRequired();
            builder.Property(o => o.AgreedTotalPrice).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.CommissionRate).HasPrecision(5, 4).IsRequired();

            // ✨ حقل عمولة المنصة الجديد الموحد
            builder.Property(o => o.PlatformCommission).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.SellerTotalPayout).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.BuyerPenaltyAmount).HasPrecision(18, 2);
            builder.Property(o => o.SellerPenaltyAmount).HasPrecision(18, 2);

            // ✨ حقل المقدم المالي الجديد
            builder.Property(o => o.DownPaymentAmount).HasPrecision(18, 2).IsRequired();

            // التحويلات الخاصة بالـ Enums والشروط
            builder.Property(o => o.DeliveryType)
                   .HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.ProposedModification);
            builder.Property(o => o.ProposedByRole)
                   .HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.Status)
                   .HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(o => o.CreatedAt).IsRequired();

            // ✨ حقول التحكم والـ Booleans الجديدة (مش محتاجة Precision لأنها داتا رقمية/تاريخ)
            builder.Property(o => o.IsSignedByBuyer).IsRequired();
            builder.Property(o => o.IsSignedBySeller).IsRequired();
            builder.Property(o => o.IsDisputed).IsRequired();
            builder.Property(o => o.IsDownPaymentPaid).IsRequired();
            builder.Property(o => o.DeliveryDate); // nullable تاريخ شحن مانيوال

            // ✨ حقول العقد الجديدة
            builder.Property(o => o.ContractTerms).HasColumnType("nvarchar(max)");
            builder.Property(o => o.ContractGeneratedAt);
            builder.Property(o => o.SellerSignedAt);
            builder.Property(o => o.BuyerSignedAt);
            builder.Property(o => o.DeclineReason).HasMaxLength(500);
            builder.Property(o => o.AgreedPricePerUnit).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.DeliveryAddress).HasMaxLength(500);
            builder.Property(o => o.DownPaymentPercentage).HasPrecision(5, 4).IsRequired();
            builder.Property(o => o.EscrowReleaseAt);
            builder.Property(o => o.ContractUrl).HasMaxLength(500);

            // الـ Indexes والعلاقات (دون أي تغيير لضمان الأمان)
            builder.HasIndex(o => o.Status);

            builder.HasOne(o => o.Listing)
                   .WithMany(l => l.Orders)
                   .HasForeignKey(o => o.ListingId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Buyer)
                   .WithMany(u => u.BuyerOrders)
                   .HasForeignKey(o => o.BuyerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Seller)
                   .WithMany(u => u.SellerOrders)
                   .HasForeignKey(o => o.SellerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(o => !o.IsDeleted);
        }
    }

    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.AvailableBalance).HasPrecision(18, 2).IsRequired();
            builder.Property(w => w.FrozenBalance).HasPrecision(18, 2).IsRequired();
            builder.Property(w => w.Currency).IsRequired().HasMaxLength(3);

            builder.HasIndex(w => w.UserId).IsUnique(); // one wallet per user

            builder.HasOne(w => w.User)
                   .WithOne(u => u.Wallet)
                   .HasForeignKey<Wallet>(w => w.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(t => t.TransactionType)
                   .HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(t => t.Status)
                   .HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(t => t.CreatedAt).IsRequired();

            builder.HasOne(t => t.Wallet)
                   .WithMany(w => w.Transactions)
                   .HasForeignKey(t => t.WalletId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Order)
                   .WithMany(o => o.Transactions)
                   .HasForeignKey(t => t.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews", t =>
                t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment);
            builder.Property(r => r.CreatedAt).IsRequired();

            // Prevent duplicate reviews from the same person on the same order.
            builder.HasIndex(r => new { r.OrderId, r.ReviewerId }).IsUnique();

            builder.HasOne(r => r.Order)
                   .WithMany(o => o.Reviews)
                   .HasForeignKey(r => r.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Reviewer)
                   .WithMany(u => u.Reviews)
                   .HasForeignKey(r => r.ReviewerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

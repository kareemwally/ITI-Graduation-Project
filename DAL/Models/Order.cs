using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    public class Order : BaseEntity, ISoftDeletable
    {
        public int ListingId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }

        public decimal AgreedQuantity { get; set; }
        public decimal AgreedTotalPrice { get; set; }

        // 💰 تعديل حقول العمولة: حقل واحد للمنصة وحقل لصافي مستحقات المورد
        public decimal CommissionRate { get; set; }           // النسبة (مثلاً 0.03 تعني 3%)
        public decimal PlatformCommission { get; set; }        // قيمة عمولة المنصة المحسوبة من الطلب
        public decimal SellerTotalPayout { get; set; }         // صافي الفلوس اللي هتروح للمورد في الآخر

        public DeliveryType? DeliveryType { get; set; }
        public decimal? BuyerPenaltyAmount { get; set; }
        public decimal? SellerPenaltyAmount { get; set; }
        public string? ProposedModification { get; set; }
        public PartyRole? ProposedByRole { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public bool IsDetailsRevealed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // 🛡️ حقول التحكم في الفلو والـ Escrow المانيوال والورقي
        public bool IsSignedByBuyer { get; set; } = false;
        public bool IsSignedBySeller { get; set; } = false;
        public bool IsDisputed { get; set; } = false;

        public decimal DownPaymentAmount { get; set; }         // قيمة المقدم الـ 10% المطلوبة أونلاين
        public bool IsDownPaymentPaid { get; set; } = false;
        public DateTime? DeliveryDate { get; set; }             // تاريخ الشحن النهائي للاكتمال التلقائي

        // 📄 حقول العقد
        public string? ContractTerms { get; set; }             // JSON — بنود الجزاءات المتفق عليها
        public DateTime? ContractGeneratedAt { get; set; }     // تاريخ إنشاء العقد
        public DateTime? SellerSignedAt { get; set; }          // تاريخ توقيع المورد
        public DateTime? BuyerSignedAt { get; set; }            // تاريخ توقيع المشتري (عند الدفع)
        public string? DeclineReason { get; set; }             // سبب رفض المورد للعقد
        public decimal AgreedPricePerUnit { get; set; }        // السعر المتفق عليه للوحدة
        public string? DeliveryAddress { get; set; }           // عنوان التوصيل
        public decimal DownPaymentPercentage { get; set; }     // نسبة العربون (مثلاً 0.10)
        public DateTime? EscrowReleaseAt { get; set; }          // تاريخ تحرير العربون
        public string? ContractUrl { get; set; }               // رابط العقد PDF على Cloudinary

        // Navigation
        public Listing Listing { get; set; } = null!;
        public User Buyer { get; set; } = null!;
        public User Seller { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    }
}
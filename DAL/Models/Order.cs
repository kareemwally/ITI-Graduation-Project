using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>
    /// Confirmed deal after a price offer is accepted. Holds the full 50/50 split commission breakdown.
    /// Platform net revenue per order = BuyerCommissionShare + SellerCommissionShare.
    /// </summary>
    public class Order : BaseEntity, ISoftDeletable
    {
        public int ListingId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }

        public decimal AgreedQuantity { get; set; }
        public decimal AgreedTotalPrice { get; set; }

        public decimal CommissionRate { get; set; }
        public decimal BuyerCommissionShare { get; set; }
        public decimal SellerCommissionShare { get; set; }
        public decimal BuyerTotalDue { get; set; }
        public decimal SellerTotalPayout { get; set; }

        public DeliveryType DeliveryType { get; set; }
        public decimal? BuyerPenaltyAmount { get; set; }
        public decimal? SellerPenaltyAmount { get; set; }
        public string? ProposedModification { get; set; }
        public PartyRole? ProposedByRole { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
        public bool IsDetailsRevealed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public Listing Listing { get; set; } = null!;
        public User Buyer { get; set; } = null!;
        public User Seller { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}

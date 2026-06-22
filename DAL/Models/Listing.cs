using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>
    /// The core marketplace listing posted by a factory. Carries the detailed fields that drive smart search.
    /// </summary>
    public class Listing : BaseEntity, ISoftDeletable
    {
        public int FactoryId { get; set; }
        public int? CategoryId { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string MaterialType { get; set; } = null!;
        public MaterialCondition MaterialCondition { get; set; }
        public decimal Quantity { get; set; }
        public string MeasureUnit { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal MinOrderQuantity { get; set; } = 1;
        public bool IsNegotiable { get; set; } = true;
        public bool IsDivisible { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PaymentMethod PreferPayMethod { get; set; }
        public string? CustomCatName { get; set; }
        public ListingStatus Status { get; set; } = ListingStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public Factory Factory { get; set; } = null!;
        public Category? Category { get; set; }
        public ICollection<ListingMedia> Media { get; set; } = new List<ListingMedia>();
        public ICollection<SavedListing> SavedByUsers { get; set; } = new List<SavedListing>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

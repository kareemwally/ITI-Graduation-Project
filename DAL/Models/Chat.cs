using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>
    /// Conversation between a buyer and a seller about a given listing.
    /// One record per (ListingId, BuyerId) — enforced by a unique index.
    /// </summary>
    public class Chat : BaseEntity
    {
        public int ListingId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public ChatStatus Status { get; set; } = ChatStatus.Open;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Listing Listing { get; set; } = null!;
        public User Buyer { get; set; } = null!;
        public User Seller { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}

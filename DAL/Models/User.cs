using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>
    /// Central account record. Anyone who enters the platform (factory owner or staff) has one row here.
    /// </summary>
    public class User : BaseEntity, ISoftDeletable
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? NationalId { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public Factory? Factory { get; set; }
        public Wallet? Wallet { get; set; }
        public ICollection<SavedListing> SavedListings { get; set; } = new List<SavedListing>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<AISearchLog> AISearchLogs { get; set; } = new List<AISearchLog>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<VerificationCase> ReviewedVerificationCases { get; set; } = new List<VerificationCase>();
        public ICollection<Chat> BuyerChats { get; set; } = new List<Chat>();
        public ICollection<Chat> SellerChats { get; set; } = new List<Chat>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Order> BuyerOrders { get; set; } = new List<Order>();
        public ICollection<Order> SellerOrders { get; set; } = new List<Order>();
    }
}

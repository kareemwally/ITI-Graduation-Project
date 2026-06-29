//using Microsoft.AspNetCore.Identity;
using DAL.Models.Common;
using DAL.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace DAL.Models
{
    public class User : IdentityUser<int>, ISoftDeletable
    {
        public string Name { get; set; } = null!;
        public string? NationalId { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
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
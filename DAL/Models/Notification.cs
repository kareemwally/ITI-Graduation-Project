using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>In-app + email notification for a user.</summary>
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? RelatedLink { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
    }
}

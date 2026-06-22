using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>
    /// Post-deal rating. Each party rates the other once
    /// (unique index on (OrderId, ReviewerId) prevents duplicates).
    /// </summary>
    public class Review : BaseEntity
    {
        public int OrderId { get; set; }
        public int ReviewerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order Order { get; set; } = null!;
        public User Reviewer { get; set; } = null!;
    }
}

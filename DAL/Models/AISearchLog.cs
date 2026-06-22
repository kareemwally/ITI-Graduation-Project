using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>
    /// Log of every smart (prompt-based) search. UserId is nullable for anonymous visitors.
    /// JSON columns hold the extracted filters and the top result ids.
    /// </summary>
    public class AISearchLog : BaseEntity
    {
        public int? UserId { get; set; }
        public string PromptText { get; set; } = null!;
        public string ExtractedFilters { get; set; } = null!;
        public int ResultsCount { get; set; }
        public string TopListingIds { get; set; } = null!;
        public string ModelVersion { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
    }
}

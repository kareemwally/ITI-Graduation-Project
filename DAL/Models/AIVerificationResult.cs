using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>
    /// AI analysis result for a verification case (1..1 with VerificationCase, enforced by a unique key).
    /// JSON columns hold semi-structured extraction output.
    /// </summary>
    public class AIVerificationResult : BaseEntity
    {
        public int VerificationCaseId { get; set; }
        public string ExtractedFields { get; set; } = null!;
        public decimal ConfidenceScore { get; set; }
        public string? Mismatches { get; set; }
        public AIRecommendation AIRecommendation { get; set; }
        public string ModelVersion { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public VerificationCase VerificationCase { get; set; } = null!;
    }
}

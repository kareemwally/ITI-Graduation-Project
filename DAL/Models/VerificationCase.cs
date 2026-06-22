using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>
    /// A factory verification attempt. One per registration plus one per re-verification,
    /// preserving the review history over time.
    /// </summary>
    public class VerificationCase : BaseEntity
    {
        public int FactoryId { get; set; }
        public int? ReviewerId { get; set; }
        public VerificationCaseStatus Status { get; set; } = VerificationCaseStatus.Pending;
        public VerificationDecision? Decision { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DecidedAt { get; set; }

        // Navigation
        public Factory Factory { get; set; } = null!;
        public User? Reviewer { get; set; }
        public AIVerificationResult? AIVerificationResult { get; set; }
    }
}

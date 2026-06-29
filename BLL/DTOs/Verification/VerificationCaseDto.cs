namespace BLL.DTOs.Verification
{
    /// <summary>API view of a verification case after a decision.</summary>
    public class VerificationCaseDto
    {
        public int Id { get; set; }
        public int FactoryId { get; set; }
        public string Status { get; set; } = null!;
        public string? Decision { get; set; }
        public int? ReviewerId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DecidedAt { get; set; }

        /// <summary>The factory's verification status after this decision (pending/verified/rejected).</summary>
        public string FactoryVerificationStatus { get; set; } = null!;
    }
}

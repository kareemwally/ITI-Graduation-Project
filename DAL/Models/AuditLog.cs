using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>
    /// Append-only record of every sensitive admin action (KYB approval, payment confirmation, ...).
    /// Payload holds a before/after JSON snapshot.
    /// </summary>
    public class AuditLog : BaseEntity
    {
        public int AdminId { get; set; }
        public string Action { get; set; } = null!;
        public string TargetEntity { get; set; } = null!;
        public int TargetId { get; set; }
        public string Payload { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User Admin { get; set; } = null!;
    }
}

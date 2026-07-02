using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    public class Dispute : BaseEntity, ISoftDeletable
    {
        public int OrderId { get; set; }
        public int RaisedById { get; set; }
        public int? MediatorId { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DisputeReason Reason { get; set; }
        public string? ResolutionNotes { get; set; }
        public DisputeStatus Status { get; set; } = DisputeStatus.Opened;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public Order Order { get; set; } = null!;
        public User RaisedBy { get; set; } = null!;
        public User? Mediator { get; set; }
        public ICollection<Document> Evidences { get; set; } = new List<Document>();
    }
}

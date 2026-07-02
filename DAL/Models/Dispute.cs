using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    public class Dispute : BaseEntity,ISoftDeletable 
    {
        public int OrderId { get; set; }
        public int RaisedById { get; set; } // المشتكي (سواء كان المشتري أو البائع)
        public int? MediatorId { get; set; } // الأدمن أو المشرف اللي هيحل النزاع

        public DisputeReason Reason { get; set; }
        public string Description { get; set; } = null!; // تفاصيل الشكوى
        public string? ResolutionNotes { get; set; } // ملاحظات الأدمن والقرار النهائي
        public DisputeStatus Status { get; set; } = DisputeStatus.Open;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Order Order { get; set; } = null!;
        public User RaisedBy { get; set; } = null!;
        public User? Mediator { get; set; }

        // لو المشتكي رافع صور أو مستندات بتثبت المشكلة (فواتير، صور خامة معيوبة)
        public ICollection<Document> Evidences { get; set; } = new List<Document>();
    }
}

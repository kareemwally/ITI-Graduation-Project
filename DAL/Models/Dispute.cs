using DAL.Models.Common;
using DAL.Models.Enums; 

namespace DAL.Models
{
    /// <summary>
    /// Tracks disputes or complaints raised by buyers or sellers regarding an order.
    /// Drives the "Disputes" section on the Dashboard.
    /// </summary>
    public class Dispute : BaseEntity, ISoftDeletable
    {
        public int OrderId { get; set; }
        public int RaisedById { get; set; } // الـ User اللي فتح الشكوى

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        // استخدمنا الـ Enum الجديد هنا بدال الـ string
        public DisputeStatus Status { get; set; } = DisputeStatus.Opened;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public Order Order { get; set; } = null!;
        public User RaisedBy { get; set; } = null!;
    }
}
using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>
    /// Uploaded document. Linked either to a Factory (KYB docs) or an Order (invoices / delivery proofs),
    /// hence both FKs are nullable. A check constraint guarantees at least one is set.
    /// </summary>
    public class Document : BaseEntity
    {
        public int? FactoryId { get; set; }
        public int? OrderId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Factory? Factory { get; set; }
        public Order? Order { get; set; }
    }
}

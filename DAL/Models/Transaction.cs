using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>Single wallet movement. Provides a full ledger of money flows.</summary>
    public class Transaction : BaseEntity
    {
        public int? OrderId { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order? Order { get; set; }
        public Wallet Wallet { get; set; } = null!;
    }
}

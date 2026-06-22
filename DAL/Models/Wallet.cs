using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>
    /// Internal wallet, one per user. Tracks available vs frozen balance. Ledger-ready for future escrow.
    /// </summary>
    public class Wallet : BaseEntity
    {
        public int UserId { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal FrozenBalance { get; set; }
        public string Currency { get; set; } = "EGP";

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

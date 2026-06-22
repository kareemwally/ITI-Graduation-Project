namespace DAL.Models
{
    /// <summary>
    /// Watchlist / favourites join entity. Composite primary key (UserId, ListingId).
    /// </summary>
    public class SavedListing
    {
        public int UserId { get; set; }
        public int ListingId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Listing Listing { get; set; } = null!;
    }
}

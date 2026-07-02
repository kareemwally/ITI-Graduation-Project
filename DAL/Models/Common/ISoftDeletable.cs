namespace DAL.Models.Common
{
    /// <summary>
    /// Marker for critical entities (Users, Factories, Listings, Orders) that should be
    /// soft-deleted instead of physically removed. A global query filter hides deleted rows.
    /// </summary>
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}

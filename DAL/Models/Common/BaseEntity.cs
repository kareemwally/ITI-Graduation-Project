namespace DAL.Models.Common
{
    /// <summary>
    /// Base type for every entity that owns a single integer identity column.
    /// Join entities with composite keys (UserRole, SavedListing) intentionally do not inherit this.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }
}

using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>Hierarchical listing category (parent -> children). Seeded data.</summary>
    public class Category : BaseEntity
    {
        public int? ParentId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}

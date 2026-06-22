using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>Egyptian governorate. Seeded on the first migration.</summary>
    public class Governorate : BaseEntity
    {
        public string Name { get; set; } = null!;

        // Navigation
        public ICollection<City> Cities { get; set; } = new List<City>();
        public ICollection<Factory> Factories { get; set; } = new List<Factory>();
    }
}

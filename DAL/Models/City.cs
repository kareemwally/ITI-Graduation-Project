using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>City belonging to a single governorate.</summary>
    public class City : BaseEntity
    {
        public int GovernorateId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation
        public Governorate Governorate { get; set; } = null!;
        public ICollection<Factory> Factories { get; set; } = new List<Factory>();
    }
}

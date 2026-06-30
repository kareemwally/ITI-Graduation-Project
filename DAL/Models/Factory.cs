using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>
    /// The company/factory. One record per company, owned by exactly one User (1..1 for the MVP).
    /// </summary>
    public class Factory : BaseEntity, ISoftDeletable
    {
        public int UserId { get; set; }
        public int? CityId { get; set; }

        public string LegalName { get; set; } = null!;
        public string CommercialRegistryNo { get; set; } = null!;
        public string TaxCardNo { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Sector { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public User Owner { get; set; } = null!;
        public City? City { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<VerificationCase> VerificationCases { get; set; } = new List<VerificationCase>();
    }
}

using DAL.Models.Enums;

namespace BLL.DTOs.Profile
{
    public class ProfileDto
    {
        // User info
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? NationalId { get; set; }
        public VerificationStatus UserVerificationStatus { get; set; }

        // Factory info
        public int? FactoryId { get; set; }
        public string? FactoryName { get; set; }
        public string? CommercialRegistryNo { get; set; }
        public string? TaxCardNo { get; set; }
        public string? Address { get; set; }
        public string? Sector { get; set; }
        public string? LogoUrl { get; set; }
        public VerificationStatus FactoryVerificationStatus { get; set; }

        // Documents
        public List<DocumentDto> Documents { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BLL.DTOs.Profile
{
    public class UpdateProfileDto
    {
        // Fields that update directly (no re-verification needed)
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Sector { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public IFormFile? LogoFile { get; set; }

        // Document files — uploading these triggers re-verification
        public IFormFile? CommercialRegistryFile { get; set; }
        public IFormFile? TaxCardFile { get; set; }
        public IFormFile? NationalIdFile { get; set; }
        public IFormFile? SelfieWithIdFile { get; set; }
    }
}

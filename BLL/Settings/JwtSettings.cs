using System.ComponentModel.DataAnnotations;

namespace BLL.Settings
{
    public class JwtSettings
    {
        [Required]
        public string SecretKey { get; set; } = null!;
        [Required]
        public string Issuer { get; set; } = null!;
        [Required]
        public string Audience { get; set; } = null!;
        [Required]
        [Range(1, 365)]
        public int DurationInDays { get; set; }
    }
}

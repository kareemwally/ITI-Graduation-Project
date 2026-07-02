using System.ComponentModel.DataAnnotations;

namespace BLL.Settings
{
    public class CloudinarySettings
    {
        [Required]
        public string CloudName { get; set; } = null!;
        [Required]
        public string ApiKey { get; set; } = null!;
        [Required]
        public string ApiSecret { get; set; } = null!;
    }
}

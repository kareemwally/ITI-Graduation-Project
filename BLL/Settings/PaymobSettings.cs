using System.ComponentModel.DataAnnotations;

namespace BLL.Settings
{
    public class PaymobSettings
    {
        [Required]
        public string ApiKey { get; set; } = null!;
        [Required]
        public string IntegrationId { get; set; } = null!;
        [Required]
        public string IframeId { get; set; } = null!;
        [Required]
        public string HmacSecret { get; set; } = null!;
    }
}

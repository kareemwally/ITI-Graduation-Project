using System.ComponentModel.DataAnnotations;

namespace BLL.Settings
{
    public class GeneralSettings
    {
        [Required]
        public string FrontendUrl { get; set; } = null!;
    }
}

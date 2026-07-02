using System.ComponentModel.DataAnnotations;

namespace BLL.Settings
{
    public class EmailSettings
    {
        [Required]
        public string SenderEmail { get; set; } = null!;
        [Required]
        public string Server { get; set; } = null!;
        [Required]
        [Range(1, 65535)]
        public int Port { get; set; }
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}

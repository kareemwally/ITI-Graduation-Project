using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.Disputes
{
    public class CreateDisputeDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;
    }
}

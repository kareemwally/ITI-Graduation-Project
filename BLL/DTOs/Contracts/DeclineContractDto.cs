using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.Contracts
{
    public class DeclineContractDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}

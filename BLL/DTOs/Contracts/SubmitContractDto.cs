using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.Contracts
{
    public class SubmitContractDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal AgreedQuantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal AgreedPricePerUnit { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string DeliveryAddress { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;

namespace BLL.Settings
{
    public class ContractSettings
    {
        [Required]
        [Range(1, 365)]
        public int EscrowDurationDays { get; set; }
        [Required]
        [Range(0.01, 1.0)]
        public decimal DownPaymentPercentage { get; set; }
        [Required]
        [Range(0.0, 1.0)]
        public decimal CommissionRate { get; set; }
        [Required]
        [Range(0.0, 1.0)]
        public decimal BuyerCancellationPenaltyRate { get; set; }
        [Required]
        [Range(0.0, 1.0)]
        public decimal SellerQualityPenaltyRate { get; set; }
        [Required]
        [Range(0.0, 1.0)]
        public decimal DelayPenaltyDailyRate { get; set; }
        [Required]
        [Range(0.0, 1.0)]
        public decimal DelayPenaltyMaxCapRate { get; set; }
        [Required]
        [Range(1, 365)]
        public int DelayTerminationBusinessDays { get; set; }
    }
}

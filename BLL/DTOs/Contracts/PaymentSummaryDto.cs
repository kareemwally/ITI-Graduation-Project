namespace BLL.DTOs.Contracts
{
    public class PaymentSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = null!;
        public string ListingTitle { get; set; } = null!;
        public decimal AgreedQuantity { get; set; }
        public decimal AgreedPricePerUnit { get; set; }
        public decimal AgreedTotalPrice { get; set; }
        public decimal DownPaymentPercentage { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public decimal CommissionRate { get; set; }
    }
}

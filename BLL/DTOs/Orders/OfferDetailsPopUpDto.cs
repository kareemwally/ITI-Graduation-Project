namespace BLL.DTOs.Orders
{
    public class OfferDetailsPopUpDto
    {
        public int OrderId { get; set; }
        public string OfferCode { get; set; } = null!;
        public string ClientCode { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public string ProductTitle { get; set; } = null!;
        public decimal QuantityRequested { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalValue { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = null!;
    }
}

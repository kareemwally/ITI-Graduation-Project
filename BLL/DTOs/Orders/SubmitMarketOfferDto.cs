namespace BLL.DTOs.Orders
{
    public class SubmitMarketOfferDto
    {
        public int ListingId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string? BuyerNote { get; set; }
    }
}

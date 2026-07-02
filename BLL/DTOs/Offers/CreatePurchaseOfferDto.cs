namespace BLL.DTOs.Offers
{
    public class CreatePurchaseOfferDto
    {
        public int ListingId { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal OfferedPricePerTon { get; set; }
        public string? BuyerMessage { get; set; }
    }
}

namespace BLL.DTOs.Offers
{
    public class PurchaseOfferDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = null!; // اسم الخامة
        public int BuyerId { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal OfferedPricePerTon { get; set; }
        public decimal TotalValue { get; set; }
        public string? BuyerMessage { get; set; }
        public string Status { get; set; } = null!; // هيرجع سترينج (pending, accepted, rejected)
    }
}
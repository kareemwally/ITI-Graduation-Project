using DAL.Models.Enums;

namespace BLL.DTOs.Listings
{
    public class UpdateListingDto
    {
        public int? CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string MaterialType { get; set; } = null!;
        public MaterialCondition MaterialCondition { get; set; }
        public decimal Quantity { get; set; }
        public string MeasureUnit { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal MinOrderQuantity { get; set; }
        public bool IsNegotiable { get; set; }
        public bool IsDivisible { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PaymentMethod PreferPayMethod { get; set; }
        public string? CustomCatName { get; set; }
        public ListingStatus Status { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

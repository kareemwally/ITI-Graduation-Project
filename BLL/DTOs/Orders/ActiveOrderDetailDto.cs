using DAL.Models.Enums;

namespace BLL.DTOs.Orders
{
    public class ActiveOrderDetailDto
    {
        public int OrderId { get; set; }
        public string OfferCode { get; set; } = null!;
        public string ClientCode { get; set; } = null!;
        public string ListingName { get; set; } = null!;
        public decimal TotalQuantity { get; set; }
        public string MeasureUnit { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public DeliveryType? DeliveryType { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

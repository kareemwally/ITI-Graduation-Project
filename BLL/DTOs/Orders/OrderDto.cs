namespace BLL.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public decimal AgreedQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}

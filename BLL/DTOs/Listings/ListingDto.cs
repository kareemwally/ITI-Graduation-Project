using DAL.Models.Enums;

namespace BLL.DTOs.Listings
{
    // The Lightweight Version
    public class ListingDto
    {
        public int Id { get; set; }
        public string FactoryAddress { get; set; } = null!;
        public string? CategoryName { get; set; }
        public string Title { get; set; } = null!;
        public string MaterialType { get; set; } = null!;
        public MaterialCondition MaterialCondition { get; set; }
        public decimal Quantity { get; set; }
        public string MeasureUnit { get; set; } = null!;

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public ListingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
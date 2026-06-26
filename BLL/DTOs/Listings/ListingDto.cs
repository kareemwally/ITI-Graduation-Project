using DAL.Models.Enums;

namespace BLL.DTOs.Listings
{
    /// <summary>Lightweight projection used in listing collections / search results.</summary>
    public class ListingDto
    {
        public int Id { get; set;}
        public int FactoryId { get; set; }
        public int? CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string MaterialType { get; set; } = null!;
        public MaterialCondition MaterialCondition { get; set; }
        public decimal Quantity { get; set; }
        public string MeasureUnit { get; set; } = null!;
        public decimal Price { get; set; }
        public ListingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

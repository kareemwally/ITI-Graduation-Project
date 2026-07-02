using DAL.Models.Enums;

namespace BLL.DTOs.Listings
{
    public class ListingDetailsDto : ListingDto
    {
        public string Description { get; set; } = null!;
        public decimal MinOrderQuantity { get; set; }
        public bool IsNegotiable { get; set; }
        public bool IsDivisible { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PaymentMethod PreferPayMethod { get; set; }
        public string? CustomCatName { get; set; }

        public string? VideoUrl { get; set; }
        public string? CertificateUrl { get; set; }

        public List<ListingMediaDto> Media { get; set; } = new();
    }

    public class ListingMediaDto
    {
        public int Id { get; set; }
        public string MediaUrl { get; set; } = null!;
        public MediaType MediaType { get; set; }
        public bool IsMain { get; set; }
    }
}

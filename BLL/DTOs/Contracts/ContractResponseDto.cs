namespace BLL.DTOs.Contracts
{
    public class ContractResponseDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = null!;
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = null!;
        public string Status { get; set; } = null!;

        public PartyInfoDto Buyer { get; set; } = null!;
        public PartyInfoDto Seller { get; set; } = null!;

        public decimal AgreedQuantity { get; set; }
        public decimal AgreedPricePerUnit { get; set; }
        public decimal AgreedTotalPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveryAddress { get; set; }

        public decimal DownPaymentPercentage { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public bool IsDownPaymentPaid { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal SellerTotalPayout { get; set; }

        public List<PenaltyClauseDto> PenaltyClauses { get; set; } = new();

        public bool IsSignedByBuyer { get; set; }
        public bool IsSignedBySeller { get; set; }
        public DateTime? ContractGeneratedAt { get; set; }
        public DateTime? SellerSignedAt { get; set; }
        public DateTime? BuyerSignedAt { get; set; }
        public string? DeclineReason { get; set; }
        public string? ContractUrl { get; set; }
    }
}

using DAL.Models.Enums;

namespace BLL.DTOs.Contracts
{
    public class ContractFormDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = null!;
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = null!;

        public PartyInfoDto Buyer { get; set; } = null!;
        public PartyInfoDto Seller { get; set; } = null!;

        // Modifiable fields (pre-filled, buyer can change)
        public decimal AgreedQuantity { get; set; }
        public decimal AgreedPricePerUnit { get; set; }
        public decimal AgreedTotalPrice { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; } = null!;

        // Fixed financial terms
        public decimal DownPaymentPercentage { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal SellerTotalPayout { get; set; }

        // Fixed penalty clauses
        public List<PenaltyClauseDto> PenaltyClauses { get; set; } = new();
    }
}

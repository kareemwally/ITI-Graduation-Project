namespace BLL.DTOs.UserDashboard
{
    public class UpdateMarketOfferDto
    {
        public decimal NewQuantity { get; set; }
        public decimal NewPricePerUnit { get; set; }
        public string? NewBuyerNote { get; set; }
    }
}

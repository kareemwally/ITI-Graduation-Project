using DAL.Models.Enums;

namespace BLL.DTOs.Chat
{
    public class ChatDetailsDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = null!;
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = null!;
        public int SellerId { get; set; }
        public string SellerName { get; set; } = null!;
        public ChatStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public List<MessageDto> Messages { get; set; } = new();
    }
}

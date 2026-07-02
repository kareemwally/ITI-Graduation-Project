using DAL.Models.Enums;

namespace BLL.DTOs.Chat
{
    public class ChatDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = null!;
        public int OtherParticipantId { get; set; }
        public string OtherParticipantCode { get; set; } = null!;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public ChatStatus Status { get; set; }
    }
}

using DAL.Models.Enums;

namespace BLL.DTOs.Chat
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public string? Content { get; set; }
        public MessageType MessageType { get; set; }
        public string? ActionUrl { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}

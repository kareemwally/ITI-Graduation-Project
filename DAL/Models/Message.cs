using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>Message inside a chat. Supports text, structured price offers and attachments.</summary>
    public class Message : BaseEntity
    {
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string? Content { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string? ActionUrl { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Chat Chat { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
}

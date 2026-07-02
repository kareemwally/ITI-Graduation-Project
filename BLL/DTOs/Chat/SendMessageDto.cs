using DAL.Models.Enums;

namespace BLL.DTOs.Chat
{
    public class SendMessageDto
    {
        public string? Content { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string? AttachmentUrl { get; set; }
    }
}

using BLL.DTOs.Chat;
using DAL.Models;

namespace BLL.Mapping.Chat
{
    public static class ChatMappings
    {
        public static MessageDto ToDto(this Message m, string senderName) => new()
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = senderName,
            Content = m.Content,
            MessageType = m.MessageType,
            ActionUrl = m.ActionUrl,
            AttachmentUrl = m.AttachmentUrl,
            IsRead = m.IsRead,
            SentAt = m.SentAt
        };

        public static ChatDto ToChatDto(this global::DAL.Models.Chat c, int currentUserId, string otherCode, string? lastMessage, int unreadCount) => new()
        {
            Id = c.Id,
            ListingId = c.ListingId,
            ListingTitle = c.Listing?.Title ?? string.Empty,
            OtherParticipantId = c.BuyerId == currentUserId ? c.SellerId : c.BuyerId,
            OtherParticipantCode = otherCode,
            LastMessage = lastMessage,
            LastMessageAt = c.Messages?.MaxBy(m => m.SentAt)?.SentAt,
            UnreadCount = unreadCount,
            Status = c.Status
        };
    }
}

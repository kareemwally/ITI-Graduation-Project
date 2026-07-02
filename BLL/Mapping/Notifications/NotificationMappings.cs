using BLL.DTOs.Notifications;
using DAL.Models;

namespace BLL.Mapping.Notifications
{
    public static class NotificationMappings
    {
        public static NotificationDto ToDto(this Notification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            RelatedLink = n.RelatedLink,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}

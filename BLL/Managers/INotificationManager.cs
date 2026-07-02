using BLL.DTOs.Common;
using BLL.DTOs.Notifications;

namespace BLL.Managers
{
    public interface INotificationManager
    {
        Task<BaseResponse<PagedResult<NotificationDto>>> GetMyNotificationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<BaseResponse<int>> GetUnreadCountAsync(int userId);
        Task<BaseResponse<bool>> MarkAsReadAsync(int notificationId, int userId);
        Task<BaseResponse<bool>> MarkAllAsReadAsync(int userId);
    }
}

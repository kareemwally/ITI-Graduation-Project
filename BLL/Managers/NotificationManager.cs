using BLL.DTOs.Common;
using BLL.DTOs.Notifications;
using BLL.Mapping.Notifications;
using DAL.Models;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class NotificationManager : INotificationManager
    {
        private readonly IUnitOfWork _uow;

        public NotificationManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<BaseResponse<PagedResult<NotificationDto>>> GetMyNotificationsAsync(int userId, int page = 1, int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _uow.Repository<Notification>().Query()
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            var total = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = notifications.Select(n => n.ToDto()).ToList();

            var result = new PagedResult<NotificationDto>
            {
                Items = dtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return BaseResponse<PagedResult<NotificationDto>>.Success(result, "تم جلب الإشعارات.");
        }

        public async Task<BaseResponse<int>> GetUnreadCountAsync(int userId)
        {
            var count = await _uow.Repository<Notification>().Query()
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return BaseResponse<int>.Success(count, "تم جلب عدد الإشعارات غير المقروءة.");
        }

        public async Task<BaseResponse<bool>> MarkAsReadAsync(int notificationId, int userId)
        {
            var repo = _uow.Repository<Notification>();
            var notification = await repo.GetByIdAsync(notificationId);

            if (notification == null || notification.UserId != userId)
                return BaseResponse<bool>.Failure("الإشعار غير موجود.");

            notification.IsRead = true;
            repo.Update(notification);
            await _uow.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "تم تحديد الإشعار كمقروء.");
        }

        public async Task<BaseResponse<bool>> MarkAllAsReadAsync(int userId)
        {
            var repo = _uow.Repository<Notification>();
            var unreadNotifications = await repo.Query()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _uow.SaveChangesAsync();
            return BaseResponse<bool>.Success(true, "تم تحديد جميع الإشعارات كمقروءة.");
        }
    }
}

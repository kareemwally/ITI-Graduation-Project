using BLL.DTOs.Common;
using BLL.DTOs.UserDashboard;
using BLL.Mapping.Dashboard;
using BLL.Mapping.Orders;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers.UsersDashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// يجلب الأرقام والإحصائيات الأربعة الرئيسية للعدادات أعلى الداشبورد
        /// </summary>
        public async Task<BaseResponse<DashboardStatsDto>> GetDashboardStatsAsync(int userId)
        {
            // 1. عدد النزاعات المفتوحة (سواء كان المستخدم مشتري أو بائع)
            var openDisputes = await _unitOfWork.Repository<Dispute>().Query()
                .CountAsync(d => d.Status == DisputeStatus.Opened &&
                                 (d.Order.BuyerId == userId || d.Order.SellerId == userId));

            // 2. عدد الرسائل غير المقروءة القادمة من الطرف الآخر
            var unreadMessages = await _unitOfWork.Repository<Message>().Query()
                .CountAsync(m => !m.IsRead && m.SenderId != userId &&
                                 (m.Chat.BuyerId == userId || m.Chat.SellerId == userId));

            // 3. عدد العروض والأسعار الجديدة غير المقروءة جوه الـ Chats
            var newOffers = await _unitOfWork.Repository<Message>().Query()
                .CountAsync(m => m.MessageType == MessageType.Offer && !m.IsRead && m.SenderId != userId &&
                                 (m.Chat.BuyerId == userId || m.Chat.SellerId == userId));

            // 4. عدد التنبيهات الجديدة غير المقروءة الخاصة بالمستخدم
            var unreadNotifications = await _unitOfWork.Repository<Notification>()
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            var statsDto = new DashboardStatsDto
            {
                OpenDisputesCount = openDisputes,
                UnreadMessagesCount = unreadMessages,
                NewOffersCount = newOffers,
                UnreadNotificationsCount = unreadNotifications
            };

            return BaseResponse<DashboardStatsDto>.Success(statsDto, "Dashboard stats retrieved successfully.");
        }

        /// <summary>
        /// يجلب الطلبات الأخيرة والأنشطة (التنبيهات) لتغذية جداول وقوائم الداشبورد
        /// </summary>
        public async Task<BaseResponse<DashboardOverviewDto>> GetDashboardOverviewAsync(int userId)
        {
            // جلب آخر 5 طلبات مر بها المستخدم بكفاءة عالية وبدون كاش في الميموري
            var recentOrdersData = await _unitOfWork.Repository<Order>().Query()
                .AsNoTracking()
                .Include(o => o.Listing)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Where(o => o.BuyerId == userId || o.SellerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // استخدام الـ Extension Method النظيفة لتحويل الداتا لـ DTO
            var recentOrders = recentOrdersData.Select(o => o.ToRecentOrderDto(userId)).ToList();

            // جلب آخر 4 تنبيهات كـ نشاطات حديثة جرت في حساب المستخدم
            var recentActivities = await _unitOfWork.Repository<Notification>().Query()
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(4)
                .Select(n => new ActivityLogDto
                {
                    Title = n.Message,
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                })
                .ToListAsync();

            var overviewDto = new DashboardOverviewDto
            {
                RecentOrders = recentOrders,
                RecentActivities = recentActivities
            };

            return BaseResponse<DashboardOverviewDto>.Success(overviewDto, "Dashboard overview retrieved successfully.");
        }

        /// <summary>
        /// ميثود مساعدة لحساب الوقت المنقضي بشكل ودي ومناسب لشاشات العرض
        /// </summary>
        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            if (timeSpan.TotalMinutes < 60) return $"منذ {Math.Max(1, (int)timeSpan.TotalMinutes)} دقيقة";
            if (timeSpan.TotalHours < 24) return $"منذ {(int)timeSpan.TotalHours} ساعة";
            return $"منذ {(int)timeSpan.TotalDays} يوم";
        }
    }

}

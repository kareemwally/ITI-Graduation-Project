using DAL.Data;
using DAL.Models.Enums;
using DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers.AdminManager
{
    public class AdminManager : IAdminManager
    {
        private readonly FayedDbContext _context;

        public AdminManager(FayedDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            // 1. إجمالي حجم التداول (مجموع أسعار الطلبات اللي متكنسلتش ومش ممسوحة)
            var totalVolume = await _context.Orders
                .Where(o => !o.IsDeleted && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.AgreedTotalPrice);

            // 2. عدد الشركات اللي في انتظار التوثيق
            var pendingCompanies = await _context.Factories
                .CountAsync(f => !f.IsDeleted && f.VerificationStatus == VerificationStatus.Pending);

            // 3. إجمالي عدد المستخدمين النشطين
            var totalUsers = await _context.Users
                .CountAsync(u => !u.IsDeleted);

            // 4. النزاعات المفتوحة الحقيقية من الجدول الجديد اللي لسه ضايفينه
            var openDisputes = await _context.Disputes
                .CountAsync(d => !d.IsDeleted && d.Status == DisputeStatus.Open);

            return new DashboardStatsDto
            {
                TotalTradingVolume = totalVolume,
                PendingCompanies = pendingCompanies,
                TotalUsers = totalUsers,
                OpenDisputes = openDisputes
            };
        }
    }
}
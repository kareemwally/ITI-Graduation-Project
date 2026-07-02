namespace DTOs.Admin
{
    public class DashboardStatsDto
    {
        // اجمالي التداول 
        public decimal TotalTradingVolume { get; set; }

        // شركات في انتظار التوثيق
        public int PendingCompanies { get; set; }

        // إجمالي المستخدمين
        public int TotalUsers { get; set; }

        // نزاعات مفتوحة نشطة
        public int OpenDisputes { get; set; }
    }
}
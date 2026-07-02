namespace BLL.DTOs.UserDashboard
{
    public class DashboardOverviewDto
    {

        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<ActivityLogDto> RecentActivities { get; set; } = new();

    }
}

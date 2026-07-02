using BLL.DTOs.Common;
using BLL.DTOs.UserDashboard;

namespace BLL.Managers.UsersDashboard
{
    public interface IDashboardService
    {
        /// <summary>
        /// يجلب الأرقام والإحصائيات الأربعة الرئيسية للعدادات أعلى الداشبورد
        /// </summary>
        Task<BaseResponse<DashboardStatsDto>> GetDashboardStatsAsync(int userId);

        /// <summary>
        /// يجلب الطلبات الأخيرة والأنشطة (التنبيهات) لتغذية جداول وقوائم الداشبورد
        /// </summary>
        Task<BaseResponse<DashboardOverviewDto>> GetDashboardOverviewAsync(int userId);
    }
}

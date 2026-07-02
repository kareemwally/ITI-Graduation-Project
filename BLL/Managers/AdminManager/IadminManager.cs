using DTOs.Admin;


namespace BLL.Managers.AdminManager
{
    public interface IAdminManager
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
using BLL.Managers.UsersDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // حماية الـ Endpoint عشان نضمن وجود التوكن والـ User Identity صح
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// استرداد إحصائيات لوحة التحكم (عدادات سريعة).
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإرجاع عدادات الإحصائيات الأربعة الرئيسية في لوحة التحكم (مثل إجمالي المستخدمين، الطلبات، الإيرادات، إلخ).
        /// الاستجابة مغلفة داخل <c>BaseResponse</c> ويتم تمرير كود الحالة ديناميكياً عبر <c>response.StatusCode</c>.
        /// يتطلب الإجراء وجود مستخدم مُوثَّق (مدير أو مشرف) ويتم استخراج معرف المستخدم من التوكن للتحقق من الصلاحية.
        /// </remarks>
        /// <returns>كائن يحتوي على إحصائيات لوحة التحكم مغلف داخل <c>BaseResponse</c>.</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // استخراج الـ User ID الموثق من داخل الـ JWT Token الصادر عند اللوجن
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var response = await _dashboardService.GetDashboardStatsAsync(userId);

            // تمرير الـ StatusCode والـ BaseResponse بشكل ديناميكي وموحد
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// استرداد نظرة عامة على لوحة التحكم (آخر الطلبات والأنشطة).
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإرجاع جدول بأحدث الطلبات وقائمة بآخر الأنشطة في النظام لعرضها في لوحة التحكم.
        /// الاستجابة مغلفة داخل <c>BaseResponse</c> ويتم تمرير كود الحالة ديناميكياً.
        /// يتطلب الإجراء وجود مستخدم مُوثَّق بصلاحيات المشرف ويتم استخراج معرف المستخدم من التوكن.
        /// </remarks>
        /// <returns>كائن يحتوي على نظرة عامة (آخر الطلبات والأنشطة) مغلف داخل <c>BaseResponse</c>.</returns>
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var response = await _dashboardService.GetDashboardOverviewAsync(userId);

            return StatusCode(response.StatusCode, response);
        }
    }
}

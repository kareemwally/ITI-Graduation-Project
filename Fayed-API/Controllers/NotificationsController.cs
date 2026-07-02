using BLL.DTOs.Common;
using BLL.DTOs.Notifications;
using BLL.Managers;
using DAL.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationManager _notificationManager;

        public NotificationsController(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }

        /// <summary>
        /// استرداد الإشعارات الخاصة بالمستخدم الحالي مع دعم التقسيم الصفحي.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإرجاع إشعارات المستخدم المُوثَّق مقسمة إلى صفحات وفقاً لمعاملات الاستعلام.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;PagedResult&lt;NotificationDto&gt;&gt;</c>؛ حيث تحتوي <c>Data</c> على الصفحة المطلوبة
        /// مع معلومات عن إجمالي عدد الإشعارات وعدد الصفحات والصفحة الحالية.
        /// يتطلب الإجراء وجود مستخدم مُوثَّق ويتم استخراج معرفه من التوكن.
        /// يتم تحديد رقم الصفحة وحجمها عبر معاملات <c>Page</c> و <c>PageSize</c> في مسار الاستعلام.
        /// </remarks>
        /// <param name="queryParams">كائن <c>PaginationQueryParamsDto</c> يحتوي على معاملات التقسيم الصفحي (Page, PageSize) من Query String.</param>
        /// <returns>صفحة من الإشعارات مع بيانات التقسيم الصفحي مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<PagedResult<NotificationDto>>>> GetMyNotifications([FromQuery] PaginationQueryParamsDto queryParams)
        {
            var userId = GetCurrentUserId();
            var response = await _notificationManager.GetMyNotificationsAsync(userId, queryParams.Page, queryParams.PageSize);
            return Ok(response);
        }

        /// <summary>
        /// الحصول على عدد الإشعارات غير المقروءة للمستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بإرجاع عدد صحيح يمثل عدد الإشعارات غير المقروءة الخاصة بالمستخدم المُوثَّق.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;int&gt;</c>؛ حيث تحتوي <c>Data</c> على العداد المطلوب.
        /// يُستخدم هذا العدد عادةً لعرض شارة (badge) في واجهة المستخدم تُنبّه بوجود إشعارات جديدة.
        /// يتطلب الإجراء وجود مستخدم مُوثَّق.
        /// </remarks>
        /// <returns>عدد الإشعارات غير المقروءة مغلف داخل <c>BaseResponse</c>.</returns>
        [HttpGet("unread-count")]
        public async Task<ActionResult<BaseResponse<int>>> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var response = await _notificationManager.GetUnreadCountAsync(userId);
            return Ok(response);
        }

        /// <summary>
        /// تعليم إشعار معين كمقروء.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بتحديث حالة إشعار محدد إلى مقروء بناءً على معرف الإشعار.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;bool&gt;</c>؛ حيث تشير <c>Data</c> إلى نجاح أو فشل العملية.
        /// يتحقق الإجراء من أن الإشعار يعود للمستخدم المُوثَّق قبل التحديث.
        /// في حال عدم وجود الإشعار أو عدم صلاحية الوصول، يتم إرجاع الحالة 404 (Not Found).
        /// </remarks>
        /// <param name="id">معرف الإشعار المراد تعليمه كمقروء.</param>
        /// <returns>قيمة منطقية (bool) تشير إلى نجاح العملية مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpPut("{id:int}/read")]
        public async Task<ActionResult<BaseResponse<bool>>> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _notificationManager.MarkAsReadAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// تعليم جميع إشعارات المستخدم الحالي كمقروءة.
        /// </summary>
        /// <remarks>
        /// يقوم هذا الإجراء بتحديث حالة جميع الإشعارات غير المقروءة الخاصة بالمستخدم المُوثَّق إلى مقروءة دفعة واحدة.
        /// الاستجابة مغلفة داخل <c>BaseResponse&lt;bool&gt;</c>.
        /// يُستخدم هذا الإجراء عادةً عند النقر على زر "قراءة الكل" في واجهة المستخدم.
        /// يتطلب الإجراء وجود مستخدم مُوثَّق ويتم استخراج معرفه من التوكن.
        /// </remarks>
        /// <returns>قيمة منطقية (bool) تشير إلى نجاح العملية مغلفة داخل <c>BaseResponse</c>.</returns>
        [HttpPut("read-all")]
        public async Task<ActionResult<BaseResponse<bool>>> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            var response = await _notificationManager.MarkAllAsReadAsync(userId);
            return Ok(response);
        }

        private int GetCurrentUserId()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(claimValue, out int userId);
            return userId;
        }
    }
}

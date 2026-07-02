using BLL.DTOs.Common;
using BLL.DTOs.Disputes;
using BLL.Managers;
using Fayed_API.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DisputesController : ControllerBase
    {
        private readonly IDisputeManager _disputeManager;

        public DisputesController(IDisputeManager disputeManager)
        {
            _disputeManager = disputeManager;
        }

        /// <summary>
        /// استرداد قائمة النزاعات الخاصة بالمستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// هذه النقطة تتطلب تسجيل الدخول (<see cref="AuthorizeAttribute"/>). يتم تغليف الاستجابة داخل <see cref="BaseResponse{T}"/> حيث T هي <see cref="List{DisputeDto}"/>.
        /// يستخدم المعرف المستخرج من التوكن لجلب النزاعات المرتبطة بالمستخدم فقط.
        /// </remarks>
        /// <returns>قائمة نزاعات المستخدم داخل BaseResponse المغلّفة.</returns>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<DisputeDto>>>> GetMyDisputes()
        {
            var userId = GetCurrentUserId();
            var response = await _disputeManager.GetMyDisputesAsync(userId);
            return Ok(response);
        }

        /// <summary>
        /// استرداد تفاصيل نزاع محدد بواسطة المعرف.
        /// </summary>
        /// <remarks>
        /// هذه النقطة تتطلب تسجيل الدخول (<see cref="AuthorizeAttribute"/>). يتم تغليف الاستجابة داخل <see cref="BaseResponse{T}"/> حيث T هي <see cref="DisputeDto"/>.
        /// يتحقق النظام من أن المستخدم الحالي هو مالك النزاع قبل إرجاع التفاصيل.
        /// </remarks>
        /// <param name="id">معرف النزاع المراد استرداد تفاصيله.</param>
        /// <returns>تفاصيل النزاع المطلوب داخل <see cref="BaseResponse{DisputeDto}"/>.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BaseResponse<DisputeDto>>> GetDetails(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _disputeManager.GetDetailsAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// إنشاء نزاع جديد.
        /// </summary>
        /// <remarks>
        /// هذه النقطة تتطلب تسجيل الدخول (<see cref="AuthorizeAttribute"/>). يتم تغليف الاستجابة داخل <see cref="BaseResponse{T}"/> حيث T هي <see cref="DisputeDto"/>.
        /// يقوم النظام بإنشاء النزاع وربطه بالمستخدم الحالي المستخرج من التوكن.
        /// يتم إرجاع الحالة المناسبة (مثل 201 Created) بناءً على نتيجة العملية.
        /// </remarks>
        /// <param name="dto">بيانات النزاع الجديد المراد إنشاؤه.</param>
        /// <returns>النزاع الذي تم إنشاؤه داخل <see cref="BaseResponse{DisputeDto}"/> مع رمز الحالة المناسب.</returns>
        [HttpPost]
        public async Task<ActionResult<BaseResponse<DisputeDto>>> Create([FromBody] CreateDisputeDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _disputeManager.CreateAsync(userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// حذف نزاع محدد بواسطة المعرف (للإدارة فقط).
        /// </summary>
        /// <remarks>
        /// تتطلب هذه النقطة تسجيل الدخول (<see cref="AuthorizeAttribute"/>) بالإضافة إلى صلاحية المشرف (AdminOnly).
        /// يتم تغليف الاستجابة داخل <see cref="BaseResponse{T}"/> حيث T هي <see cref="bool"/>.
        /// فقط المستخدمون الذين لديهم صلاحية المشرف يمكنهم حذف النزاعات.
        /// </remarks>
        /// <param name="id">معرف النزاع المراد حذفه.</param>
        /// <returns>قيمة منطقية تشير إلى نجاح أو فشل عملية الحذف داخل BaseResponse المغلّفة.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(int id)
        {
            var response = await _disputeManager.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        private int GetCurrentUserId()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(claimValue, out int userId);
            return userId;
        }
    }
}

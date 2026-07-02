using BLL.DTOs.Common;
using BLL.DTOs.Profile;
using BLL.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileManager _profileManager;

        public ProfileController(IProfileManager profileManager)
        {
            _profileManager = profileManager;
        }

        /// <summary>
        /// استرداد الملف الشخصي للمستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// هذه النقطة تتطلب تسجيل الدخول (<see cref="AuthorizeAttribute"/>). يتم تغليف الاستجابة داخل <see cref="BaseResponse{T}"/> حيث T هي <see cref="ProfileDto"/>.
        /// يتم استخراج معرف المستخدم من التوكن لجلب بيانات الملف الشخصي المرتبطة به.
        /// </remarks>
        /// <returns>الملف الشخصي للمستخدم الحالي داخل <see cref="BaseResponse{ProfileDto}"/>.</returns>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<ProfileDto>>> GetProfile()
        {
            var userId = GetCurrentUserId();
            var response = await _profileManager.GetProfileAsync(userId);
            return Ok(response);
        }

        /// <summary>
        /// تحديث الملف الشخصي للمستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// هذه النقطة تتطلب تسجيل الدخول (<see cref="AuthorizeAttribute"/>). يتم تغليف الاستجابة داخل <see cref="BaseResponse{T}"/> حيث T هي <see cref="ProfileDto"/>.
        /// في حالة إعادة رفع ملفات المستندات (مثل الهوية أو إثبات العنوان)، يتم إعادة تعيين حالة التحقق إلى "قيد الانتظار" (Pending) لحين مراجعتها من قبل المشرف.
        /// </remarks>
        /// <param name="dto">بيانات الملف الشخصي الجديدة مع إمكانية رفع الملفات.</param>
        /// <returns>الملف الشخصي المحدث داخل <see cref="BaseResponse{ProfileDto}"/> مع رمز الحالة المناسب.</returns>
        [HttpPut]
        public async Task<ActionResult<BaseResponse<ProfileDto>>> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _profileManager.UpdateProfileAsync(userId, dto);
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

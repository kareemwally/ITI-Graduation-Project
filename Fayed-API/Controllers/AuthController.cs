using BLL.DTOs.Authentication;
using BLL.DTOs.Common;
using BLL.Managers.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// تسجيل مصنع جديد في المنصة.
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ولا تتطلب تفويضاً مسبقاً.
        /// تستخدم <c>BaseResponse&lt;AuthResultDto&gt;</c> لتغليف الاستجابة.
        /// عند نجاح التسجيل يتم إرسال بريد إلكتروني لتأكيد الحساب قبل أن يتمكن المصنع من تسجيل الدخول.
        /// يستقبل الطلب بيانات النموذج مع رفع الملفات (صور المصنع).
        /// </remarks>
        /// <param name="request">كائن يحتوي على بيانات التسجيل: اسم المستخدم، البريد الإلكتروني، كلمة المرور، الملفات التجارية، صور المصنع، والعنوان.</param>
        /// <returns>كائن <c>BaseResponse&lt;AuthResultDto&gt;</c> يحمل نتيجة التسجيل بما في ذلك رمز JWT في حالة النجاح.</returns>
        [HttpPost("register-factory")]
        public async Task<IActionResult> RegisterFactory([FromForm] FayedRegisterFactoryRequest request)
        {
            var result = await _authService.RegisterFactoryAsync(request);
            return Ok(result);
        
        }
        /// <summary>
        /// تأكيد البريد الإلكتروني للمستخدم بعد التسجيل.
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ولا تتطلب تفويضاً.
        /// تستخدم <c>BaseResponse&lt;string&gt;</c> لتغليف الاستجابة.
        /// يتم استدعاؤها عبر الرابط المرسل في البريد الإلكتروني لتأكيد الحساب.
        /// بعد التأكيد يصبح بإمكان المصنع تسجيل الدخول إلى المنصة.
        /// </remarks>
        /// <param name="userId">معرف المستخدم (GUID) المراد تأكيد بريده.</param>
        /// <param name="token">رمز التأكيد المشفر المرسل عبر البريد الإلكتروني.</param>
        /// <returns>كائن <c>BaseResponse&lt;string&gt;</c> يحمل رسالة نجاح أو فشل عملية التأكيد.</returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            return Ok(result);
        }
        /// <summary>
        /// تسجيل دخول المصنع إلى المنصة.
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ولا تتطلب تفويضاً مسبقاً.
        /// تستخدم <c>BaseResponse&lt;AuthResultDto&gt;</c> لتغليف الاستجابة.
        /// يجب أن يكون البريد الإلكتروني مؤكداً مسبقاً عبر <c>ConfirmEmail</c>.
        /// عند النجاح يتم إرجاع رمز JWT للوصول إلى النقاط المحمية.
        /// </remarks>
        /// <param name="request">كائن يحتوي على البريد الإلكتروني وكلمة المرور.</param>
        /// <returns>كائن <c>BaseResponse&lt;AuthResultDto&gt;</c> يحتوي على رمز JWT ومعلومات المصنع عند نجاح تسجيل الدخول.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FayedLoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(result);
        }
        /// <summary>
        /// إرسال رابط إعادة تعيين كلمة المرور إلى البريد الإلكتروني للمستخدم.
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ولا تتطلب تفويضاً.
        /// تستخدم <c>BaseResponse&lt;string&gt;</c> لتغليف الاستجابة.
        /// يتم إنشاء رمز إعادة تعيين وإرساله عبر البريد الإلكتروني المسجل.
        /// الرابط يحتوي على الرمز الذي يُستخدم في نقطة <c>ResetPassword</c>.
        /// </remarks>
        /// <param name="request">كائن يحتوي على البريد الإلكتروني للمستخدم المراد إعادة تعيين كلمة المرور الخاصة به.</param>
        /// <returns>كائن <c>BaseResponse&lt;string&gt;</c> يحمل رسالة تأكيد إرسال رابط إعادة التعيين.</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] FayedForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            return Ok(result);
        }

        /// <summary>
        /// إعادة تعيين كلمة المرور باستخدام الرمز المرسل عبر البريد الإلكتروني.
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ولا تتطلب تفويضاً.
        /// تستخدم <c>BaseResponse&lt;string&gt;</c> لتغليف الاستجابة.
        /// يتم التحقق من صحة الرمز ثم تحديث كلمة المرور في قاعدة البيانات.
        /// هذا هو الخطوة النهائية في سير عمل استعادة كلمة المرور بعد <c>ForgotPassword</c>.
        /// </remarks>
        /// <param name="request">كائن يحتوي على البريد الإلكتروني والرمز الجديد وكلمة المرور الجديدة وتأكيدها.</param>
        /// <returns>كائن <c>BaseResponse&lt;string&gt;</c> يحمل رسالة نجاح أو فشل عملية إعادة التعيين.</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] FayedResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            return Ok(result);
        }

    }
}


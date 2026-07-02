using BLL.DTOs.Common;
using BLL.Managers;
using Microsoft.AspNetCore.Mvc;

namespace Fayed_API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsWebhookController : ControllerBase
    {
        private readonly IContractManager _contractManager;

        public PaymentsWebhookController(IContractManager contractManager)
        {
            _contractManager = contractManager;
        }

        /// <summary>
        /// استقبال رد الدفع من Paymob (ويب هوك).
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ولا تتطلب تسجيل الدخول أو أي تفويض لأنها تُستدعى من خدمة Paymob الخارجية.
        /// يتم تغليف الاستجابة داخل <see cref="BaseResponse{T}"/> حيث T هي <see cref="bool"/>.
        /// تدعم هذه النقطة مسارين: ويب هوك Paymob الفعلي (<c>webhook</c>) والمحاكاة (<c>simulated-callback</c>) لإعادة التوجيه من الواجهة الأمامية.
        /// يقوم النظام بتأكيد حالة الدفع وتحديث حالة العقد بناءً على المعاملات المرسلة.
        /// </remarks>
        /// <param name="orderId">معرف الطلب المراد تأكيد دفعه.</param>
        /// <param name="status">حالة الدفع المرسلة من Paymob (مثل completed أو failed).</param>
        /// <param name="paymobPayload">الحمولة الكاملة المرسلة من Paymob (اختياري).</param>
        /// <returns>قيمة منطقية تشير إلى نجاح أو فشل معالجة رد الدفع داخل BaseResponse المغلّفة.</returns>
        [HttpPost("webhook")]
        [HttpPost("simulated-callback")]
        public async Task<ActionResult<BaseResponse<bool>>> PaymentCallback(
            [FromQuery] int orderId,
            [FromQuery] string status,
            [FromBody] object? paymobPayload = null)
        {
            var response = await _contractManager.ConfirmPaymentCallbackAsync(orderId, status);
            return StatusCode(response.StatusCode, response);
        }
    }
}

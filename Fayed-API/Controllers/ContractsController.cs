using BLL.DTOs.Common;
using BLL.DTOs.Contracts;
using BLL.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders/{orderId:int}/contract")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractManager _contractManager;

        public ContractsController(IContractManager contractManager)
        {
            _contractManager = contractManager;
        }

        /// <summary>
        /// يستعيد نموذج العقد الخاص بطلب معين ليتم تعبئته من قبل المستخدم.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="ContractFormDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] على مستوى الـ Controller.
        /// المستهلك (المشتري) هو من يستدعي هذا endpoint لاستعراض بيانات العقد الأولية قبل التقديم.
        /// هذه هي الخطوة الأولى في دورة حياة العقد: form → submit → accept → pay → confirm-delivery.
        /// </remarks>
        /// <param name="orderId">معرف الطلب الفريد المرتبط بالعقد.</param>
        /// <returns>نموذج العقد مع البيانات الأولية ضمن هيكل BaseResponse المغلف.</returns>
        [HttpGet("form")]
        public async Task<ActionResult<BaseResponse<ContractFormDto>>> GetForm(int orderId)
        {
            var userId = GetCurrentUserId();
            var response = await _contractManager.GetFormAsync(orderId, userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يقدم العقد من قبل المشتري (المستهلك) بعد تعبئة النموذج وتوقيعه.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="ContractResponseDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] على مستوى الـ Controller.
        /// يتم استدعاؤه من قبل المشتري لتقديم العقد مسببًا تغيير حالة الطلب إلى ContractReview.
        /// هذه هي الخطوة الثانية في دورة حياة العقد: form → submit (buyer signs, status → ContractReview) → accept → pay → confirm-delivery.
        /// </remarks>
        /// <param name="orderId">معرف الطلب الفريد المرتبط بالعقد.</param>
        /// <param name="dto">كائن DTO يحتوي على بيانات العقد المقدمة من المشتري (التوقيع، الشروط، إلخ).</param>
        /// <returns>كائن استجابة العقد مع البيانات المقدمة ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPut]
        public async Task<ActionResult<BaseResponse<ContractResponseDto>>> SubmitContract(int orderId, [FromBody] SubmitContractDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _contractManager.SubmitContractAsync(orderId, userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يستعيد العقد الحالي المرتبط بطلب معين.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="ContractResponseDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] على مستوى الـ Controller.
        /// يمكن لكل من المشتري والبائع استعراض العقد بعد تقديمه للتحقق من الشروط والحالة الحالية.
        /// </remarks>
        /// <param name="orderId">معرف الطلب الفريد المرتبط بالعقد.</param>
        /// <returns>بيانات العقد الحالية ضمن هيكل BaseResponse المغلف.</returns>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<ContractResponseDto>>> GetContract(int orderId)
        {
            var userId = GetCurrentUserId();
            var response = await _contractManager.GetContractAsync(orderId, userId);
            return Ok(response);
        }

        /// <summary>
        /// يقبل البائع (المصنع) العقد المقدم من المشتري بتوقيعه.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="ContractResponseDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] على مستوى الـ Controller.
        /// يتم استدعاؤه من قبل البائع (المصنع) لقبول العقد مسببًا تغيير حالة الطلب إلى PaymentPending.
        /// هذه هي الخطوة الثالثة في دورة حياة العقد: form → submit → accept (seller signs, status → PaymentPending) → pay → confirm-delivery.
        /// </remarks>
        /// <param name="orderId">معرف الطلب الفريد المرتبط بالعقد.</param>
        /// <returns>كائن استجابة العقد بعد القبول ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPut("accept")]
        public async Task<ActionResult<BaseResponse<ContractResponseDto>>> AcceptContract(int orderId)
        {
            var userId = GetCurrentUserId();
            var response = await _contractManager.AcceptContractAsync(orderId, userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يرفض البائع (المصنع) العقد المقدم من المشتري مع إرفاق سبب الرفض.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="ContractResponseDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] على مستوى الـ Controller.
        /// يتم استدعاؤه من قبل البائع (المصنع) لرفض العقد مع تقديم سبب الرفض في DTO.
        /// يؤدي الرفض إلى إنهاء دورة حياة العقد ويعيد الطلب إلى حالة سابقة أو يلغي المعاملة.
        /// </remarks>
        /// <param name="orderId">معرف الطلب الفريد المرتبط بالعقد.</param>
        /// <param name="dto">كائن DTO يحتوي على سبب الرفض.</param>
        /// <returns>كائن استجابة العقد بعد الرفض ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPut("decline")]
        public async Task<ActionResult<BaseResponse<ContractResponseDto>>> DeclineContract(int orderId, [FromBody] DeclineContractDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _contractManager.DeclineContractAsync(orderId, userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يستعيد ملخص الدفع الخاص بالعقد المرتبط بطلب معين.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="PaymentSummaryDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] على مستوى الـ Controller.
        /// يمكن لكل من المشتري والبائع استعراض ملخص الدفع لمعرفة المبالغ المستحقة والمدفوعة والجدول الزمني للدفع.
        /// </remarks>
        /// <param name="orderId">معرف الطلب الفريد المرتبط بالعقد.</param>
        /// <returns>ملخص الدفع (المبلغ الإجمالي، الدفعة المقدمة، الرصيد المتبقي) ضمن هيكل BaseResponse المغلف.</returns>
        [HttpGet("payment-summary")]
        public async Task<ActionResult<BaseResponse<PaymentSummaryDto>>> GetPaymentSummary(int orderId)
        {
            var userId = GetCurrentUserId();
            var response = await _contractManager.GetPaymentSummaryAsync(orderId, userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يقوم المشتري بدفع الدفعة المقدمة (الدفعة الأولى) للعقد.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="PaymentResponseDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] على مستوى الـ Controller.
        /// يتم استدعاؤه من قبل المشتري لبدء عملية الدفع، مما يؤدي إلى تغيير حالة الطلب إلى PaymentProcessing أو ما يعادلها.
        /// هذه هي الخطوة الرابعة في دورة حياة العقد: form → submit → accept → pay (down payment) → confirm-delivery.
        /// </remarks>
        /// <param name="orderId">معرف الطلب الفريد المرتبط بالعقد.</param>
        /// <returns>كائن استجابة الدفع يحتوي على رابط الدفع أو تأكيد المعاملة ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPost("pay")]
        public async Task<ActionResult<BaseResponse<PaymentResponseDto>>> Pay(int orderId)
        {
            var userId = GetCurrentUserId();
            var response = await _contractManager.InitiatePaymentAsync(orderId, userId);
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

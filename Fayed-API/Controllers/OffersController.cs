using BLL.DTOs.Common;
using BLL.DTOs.Orders;
using BLL.DTOs.UserDashboard;
using BLL.Managers;
using DAL.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferManager _offerManager;

        public OffersController(IOfferManager offerManager)
        {
            _offerManager = offerManager;
        }

        /// <summary>
        /// يستعيد جميع العروض التي أرسلها المستخدم الحالي (بدون ترقيم).
        /// </summary>
        [HttpGet("sent")]
        public async Task<IActionResult> GetSentOffers()
        {
            var userId = GetCurrentUserId();
            var response = await _offerManager.GetSentOffersAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يستعيد قائمة العروض التي تلقاها المستخدم الحالي (بصفته مشتريًا).
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="PagedResult{OrderDashboardListDto}"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// يتم استخراج معرف المستخدم من المطالبة ClaimTypes.NameIdentifier في التوكن.
        /// </remarks>
        /// <param name="queryParams">معلمات ترقيم الصفحات (Page, PageSize) المستخدمة في تقسيم النتائج.</param>
        /// <returns>قائمة مرتبة من العروض المستلمة ضمن هيكل BaseResponse المغلف.</returns>
        [HttpGet("received")]
        public async Task<ActionResult<BaseResponse<PagedResult<OrderDashboardListDto>>>> GetReceivedOffers([FromQuery] PaginationQueryParamsDto queryParams)
        {
            var userId = GetCurrentUserId();
            var response = await _offerManager.GetReceivedOffersAsync(userId, queryParams.Page, queryParams.PageSize);
            return Ok(response);
        }

        /// <summary>
        /// يستعيد تفاصيل عرض معين بناءً على معرف العرض ومعرف المستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="OfferDetailsPopUpDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// يتم استخراج معرف المستخدم من المطالبة ClaimTypes.NameIdentifier للتحقق من ملكية العرض.
        /// </remarks>
        /// <param name="id">معرف العرض الفريد المطلوب عرض تفاصيله.</param>
        /// <returns>تفاصيل العرض (بما في ذلك بيانات المنتج والسعر والحالة) ضمن هيكل BaseResponse المغلف.</returns>
        [HttpGet("{id:int}/details")]
        public async Task<ActionResult<BaseResponse<OfferDetailsPopUpDto>>> GetOfferDetails(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _offerManager.GetOfferDetailsAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// يقدم عرضًا جديدًا من السوق من قبل مصنع موثق.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي bool.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] وسمة السياسة [Authorize(Policy = "VerifiedFactoryOnly")]
        /// للسماح فقط للمصانع الموثقة بتقديم العروض.
        /// يتم إنشاء العرض في حالة انتظار المراجعة (Pending) لينتظر قبول المشتري.
        /// </remarks>
        /// <param name="dto">كائن DTO يحتوي على بيانات العرض المطلوب تقديمه (مثل معرف المنتج والسعر والكمية).</param>
        /// <returns>قيمة صحيحة (true) تشير إلى نجاح عملية التقديم ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPost("submit")]
        [Authorize(Policy = "VerifiedFactoryOnly")]
        public async Task<ActionResult<BaseResponse<bool>>> SubmitOfferFromMarket([FromBody] SubmitMarketOfferDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _offerManager.SubmitOfferFromMarketAsync(userId, dto);
            return Ok(response);
        }

        /// <summary>
        /// يحدّث عرضًا قائمًا من قبل المصنع الموثق الذي أنشأه.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي bool.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] وسمة السياسة [Authorize(Policy = "VerifiedFactoryOnly")]
        /// للسماح فقط للمصانع الموثقة بتحديث عروضها.
        /// يمكن تحديث العرض فقط إذا كان لا يزال في حالة معلقة (Pending) قبل أن يقبله المشتري.
        /// </remarks>
        /// <param name="id">معرف العرض الفريد المطلوب تحديثه.</param>
        /// <param name="dto">كائن DTO يحتوي على بيانات التحديث (السعر، الكمية، إلخ).</param>
        /// <returns>قيمة صحيحة (true) تشير إلى نجاح عملية التحديث ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPut("{id:int}/update")]
        [Authorize(Policy = "VerifiedFactoryOnly")]
        public async Task<ActionResult<BaseResponse<bool>>> UpdateOffer(int id, [FromBody] UpdateMarketOfferDto dto)
        {
            var userId = GetCurrentUserId();
            var response = await _offerManager.UpdateOfferAsync(id, userId, dto);
            return Ok(response);
        }

        /// <summary>
        /// يقبل المصنع الموثق عرضًا واردًا (بصفته بائعًا).
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي bool.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize] وسمة السياسة [Authorize(Policy = "VerifiedFactoryOnly")]
        /// للسماح فقط للمصانع الموثقة بقبول العروض.
        /// عند القبول، ينتقل العرض إلى حالة مقبول (Accepted) مما يؤدي إلى إنشاء طلب (Order).
        /// </remarks>
        /// <param name="id">معرف العرض الفريد المطلوب قبوله.</param>
        /// <returns>قيمة صحيحة (true) تشير إلى نجاح عملية القبول ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPut("{id:int}/accept")]
        [Authorize(Policy = "VerifiedFactoryOnly")]
        public async Task<ActionResult<BaseResponse<bool>>> AcceptOffer(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _offerManager.AcceptOfferAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// يرفض أو يلغي عرضًا بناءً على دور المستخدم (مشتري يرفض أو بائع يلغي).
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي bool.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// إذا كان المستخدم هو البائع، يتم إلغاء العرض (Cancel). إذا كان المستخدم هو المشتري، يتم رفض العرض (Reject).
        /// ينتقل العرض إلى حالة ملغي (Cancelled) أو مرفوض (Rejected) ولا يمكن متابعة الإجراءات عليه.
        /// </remarks>
        /// <param name="id">معرف العرض الفريد المطلوب رفضه أو إلغاؤه.</param>
        /// <returns>قيمة صحيحة (true) تشير إلى نجاح عملية الرفض أو الإلغاء ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPut("{id:int}/reject-or-cancel")]
        public async Task<ActionResult<BaseResponse<bool>>> RejectOrCancelOffer(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _offerManager.RejectOrCancelOfferAsync(id, userId);
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

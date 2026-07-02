using BLL.DTOs.Common;
using BLL.DTOs.Orders;
using BLL.DTOs.UserDashboard;
using BLL.Managers;
using DAL.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderManager _orderManager;

        public OrdersController(IOrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        /// <summary>
        /// ينشئ طلبًا جديدًا في النظام.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي كائن الطلب.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// يتم إنشاء الطلب بناءً على البيانات المقدمة من المشتري ويتحقق النظام من صحة المنتج والبائع والمخزون.
        /// </remarks>
        /// <param name="dto">كائن DTO يحتوي على بيانات الطلب (مثل معرف المنتج والكمية وعنوان التوصيل).</param>
        /// <returns>كائن الطلب المُنشأ ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var response = await _orderManager.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يستعيد طلبًا محددًا بناءً على معرفه الفريد.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي كائن الطلب.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// يمكن لكل من المشتري والبائع الاطلاع على تفاصيل الطلب بعد التحقق من ملكيتهما له.
        /// </remarks>
        /// <param name="id">معرف الطلب الفريد المطلوب استعادته.</param>
        /// <returns>تفاصيل الطلب الكاملة ضمن هيكل BaseResponse المغلف.</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var response = await _orderManager.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يستعيد قائمة مشتريات المستخدم الحالي (جميع الطلبات بدون ترقيم).
        /// </summary>
        [HttpGet("my-purchases")]
        public async Task<IActionResult> GetMyPurchases()
        {
            var userId = GetCurrentUserId();
            var response = await _orderManager.GetMyPurchasesAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يستعيد قائمة مبيعات المستخدم الحالي (الطلبات التي قام ببيعها).
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="ConfirmedOrderDashboardDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// يتم استخراج معرف المستخدم من المطالبة ClaimTypes.NameIdentifier لتصفية الطلبات التي تخصه كبائع.
        /// </remarks>
        [HttpGet("my-sales")]
        public async Task<IActionResult> GetMySales()
        {
            var userId = GetCurrentUserId();
            var response = await _orderManager.GetMySalesAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// يستعيد تفاصيل طلب نشط بناءً على معرف الطلب للمستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي <see cref="ActiveOrderDetailDto"/>.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// يتم استخراج معرف المستخدم من المطالبة ClaimTypes.NameIdentifier للتحقق من ملكية الطلب (كمشترٍ أو بائع).
        /// </remarks>
        /// <param name="id">معرف الطلب الفريد المطلوب عرض تفاصيله النشطة.</param>
        /// <returns>تفاصيل الطلب النشط (الحالة، المنتج، السعر، إلخ) ضمن هيكل BaseResponse المغلف.</returns>
        [HttpGet("{id:int}/details")]
        public async Task<ActionResult<BaseResponse<ActiveOrderDetailDto>>> GetActiveOrderDetail(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _orderManager.GetActiveOrderDetailAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// يكمل طلبًا نشطًا (للبائع) — يخصم الكمية من المخزون ويحول الطلب إلى مكتمل.
        /// </summary>
        [HttpPut("{id:int}/complete")]
        public async Task<ActionResult<BaseResponse<bool>>> CompleteOrder(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _orderManager.CompleteOrderAsync(id, userId);
            return Ok(response);
        }

        /// <summary>
        /// يلغي طلبًا نشطًا بناءً على معرف الطلب للمستخدم الحالي.
        /// </summary>
        /// <remarks>
        /// يغلف هذا endpoint الاستجابة في <see cref="BaseResponse{T}"/> حيث T هي bool.
        /// يتطلب هذا endpoint تفويض المستخدم عبر السمة [Authorize].
        /// يمكن للمشتري إلغاء الطلب قبل أن ينتقل إلى حالة الدفع (PaymentPending).
        /// بعد الإلغاء، لا يمكن متابعة أي إجراءات أخرى على الطلب.
        /// </remarks>
        /// <param name="id">معرف الطلب الفريد المطلوب إلغاؤه.</param>
        /// <returns>قيمة صحيحة (true) تشير إلى نجاح عملية الإلغاء ضمن هيكل BaseResponse المغلف.</returns>
        [HttpPut("{id:int}/cancel")]
        public async Task<ActionResult<BaseResponse<bool>>> CancelOrder(int id)
        {
            var userId = GetCurrentUserId();
            var response = await _orderManager.CancelOrderAsync(id, userId);
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

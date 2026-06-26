using BLL.DTOs.Orders;
using BLL.Managers;
using DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderManager _orderManager;

        public OrdersController(IOrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        // 1. إنشاء طلب جديد (المصنع بيطلب مخلفات)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var result = await _orderManager.CreateAsync(dto);
            // بيرجع 201 Created ومعاه بيانات الطلب الجديد
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
        }

        // 2. جلب تفاصيل طلب معين بالـ ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var result = await _orderManager.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "الطلب ده مش موجود" });

            return Ok(result);
        }

        // 3. جلب كل طلبات مصنع معين (عشان نعرضها في الداش بورد بتاعته)
        [HttpGet("factory/{factoryId}")]
        public async Task<IActionResult> GetFactoryOrders(int factoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _orderManager.GetOrdersByFactoryAsync(factoryId, page, pageSize);
            return Ok(result);
        }

        // 4. تغيير حالة الطلب (مثلاً من PendingPayment لـ Completed)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] OrderStatus newStatus)
        {
            var success = await _orderManager.UpdateStatusAsync(id, newStatus);
            if (!success)
                return NotFound(new { message = "الطلب ده مش موجود عشان تتغير حالته" });

            return NoContent(); // 204 No Content (معناها العملية نجحت ومفيش داتا محتاجين نرجعها)
        }
    }
}

using BLL.DTOs.Offers;
using BLL.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // لازم يكون عامل لوجين عشان يكلم الـ Endpoints دي
    public class PurchaseOffersController : ControllerBase
    {
        private readonly IPurchaseOfferManager _offerManager;

        public PurchaseOffersController(IPurchaseOfferManager offerManager)
        {
            _offerManager = offerManager;
        }

        // 1. إرسال عرض شراء جديد
        [HttpPost("create")]
        public async Task<IActionResult> CreateOffer([FromBody] CreatePurchaseOfferDto dto)
        {
            var rawBuyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(rawBuyerId) || !int.TryParse(rawBuyerId, out int buyerId))
            {
                return Unauthorized(new { IsSuccess = false, Message = "غير مصرح لك بالقيام بهذه العملية." });
            }

            var response = await _offerManager.CreateOfferAsync(buyerId, dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        // 2. رد المورد على العقد (قبول أو رفض)
        [HttpPut("{id}/respond")]
        public async Task<IActionResult> RespondToOffer(int id, [FromQuery] bool isAccepted)
        {
            var rawSellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(rawSellerId) || !int.TryParse(rawSellerId, out int sellerId))
            {
                return Unauthorized(new { IsSuccess = false, Message = "غير مصرح لك بالقيام بهذه العملية." });
            }

            var response = await _offerManager.RespondToOfferAsync(id, sellerId, isAccepted);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        // 3. جلب العروض اللي أنا (كمشتري) بعتها
        [HttpGet("my-sent-offers")]
        public async Task<IActionResult> GetMySentOffers()
        {
            var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(rawUserId) || !int.TryParse(rawUserId, out int userId))
            {
                return Unauthorized(new { IsSuccess = false, Message = "غير مصرح لك بالقيام بهذه العملية." });
            }

            var response = await _offerManager.GetBuyerOffersAsync(userId);
            return Ok(response);
        }

        // 4. جلب العروض اللي جاتني (كمورد) على إعلانات مصنعي
        [HttpGet("my-received-offers")]
        public async Task<IActionResult> GetMyReceivedOffers()
        {
            var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(rawUserId) || !int.TryParse(rawUserId, out int userId))
            {
                return Unauthorized(new { IsSuccess = false, Message = "غير مصرح لك بالقيام بهذه العملية." });
            }

            var response = await _offerManager.GetSellerOffersAsync(userId);
            return Ok(response);
        }
    }
}
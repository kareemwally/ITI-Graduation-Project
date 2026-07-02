using BLL.DTOs.Common;
using BLL.DTOs.Listings;
using BLL.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingManager _listingManager;

        public ListingsController(IListingManager listingManager)
        {
            _listingManager = listingManager;
        }

        /// <summary>
        /// استرجاع جميع الخامات المنشورة في السوق مع الفلاتر.
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ومتاحة للجميع بدون تفويض.
        /// تدعم الفلترة حسب: نوع الخامة، الموقع، التصنيف، الكمية، السعر، والترتيب.
        /// </remarks>
        /// <param name="filter">كائن يحتوي على معاملات الفلترة والترقيم والترتيب.</param>
        /// <returns>كائن <c>BaseResponse&lt;PaginatedResult&lt;ListingDto&gt;&gt;</c> يحتوي على قائمة الخامات المنشورة.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPublished([FromQuery] PublishedListingsFilterDto filter)
        {
            var response = await _listingManager.GetPublishedAsync(filter);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// استرجاع خامات المصنع المسجل دخوله حالياً (جميع المنتجات بدون ترقيم).
        /// </summary>
        [HttpGet("my-listings")]
        [Authorize]
        public async Task<IActionResult> GetMyListings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _listingManager.GetByUserFactoryAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// استرجاع تفاصيل خامة محددة بواسطة المعرف.
        /// </summary>
        /// <remarks>
        /// هذه النقطة عامة ومتاحة للجميع بدون تفويض.
        /// تستخدم <c>BaseResponse&lt;ListingDto&gt;</c> لتغليف الاستجابة.
        /// ترجع جميع تفاصيل الخامة بما في ذلك الصور والمواصفات والكمية والسعر والمصنع المالك.
        /// </remarks>
        /// <param name="id">المعرف الرقمي للخامة المراد استرجاعها.</param>
        /// <returns>كائن <c>BaseResponse&lt;ListingDto&gt;</c> يحتوي على تفاصيل الخامة المطلوبة.</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _listingManager.GetByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// إنشاء خامة جديدة في السوق.
        /// </summary>
        /// <remarks>
        /// هذه النقطة محمية وتتطلب تفويضاً <c>[Authorize]</c>.
        /// تستخدم <c>BaseResponse&lt;ListingDto&gt;</c> لتغليف الاستجابة.
        /// تستخدم <c>[FromForm]</c> لدعم رفع صور الخامة والملفات المرافقة.
        /// بعد الإنشاء تكون الخامة في حالة مسودة (غير منشورة) حتى يتم استدعاء نقطة <c>Publish</c>.
        /// سير عمل الحالة: مسودة ← منشورة ← مبيعة / محجوزة (حسب تدفق الإسكرو).
        /// </remarks>
        /// <param name="dto">كائن يحمل بيانات الخامة الجديدة: الاسم، الوصف، الكمية، السعر، نوع الخامة، المعرف التصنيفي، والصور.</param>
        /// <returns>كائن <c>BaseResponse&lt;ListingDto&gt;</c> يحتوي على الخامة المنشأة مع حالتها الابتدائية (مسودة).</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateListingDto dto)
        {
            var response = await _listingManager.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// تحديث بيانات خامة موجودة.
        /// </summary>
        /// <remarks>
        /// هذه النقطة محمية وتتطلب تفويضاً <c>[Authorize]</c>.
        /// تستخدم <c>BaseResponse&lt;ListingDto&gt;</c> لتغليف الاستجابة.
        /// تستخدم <c>[FromForm]</c> لدعم تحديث الصور والملفات.
        /// يمكن تعديل الخامة فقط إذا كانت في حالة مسودة ولم يتم نشرها بعد.
        /// التحقق من ملكية الخامة يتم داخل طبقة المدير.
        /// </remarks>
        /// <param name="id">المعرف الرقمي للخامة المراد تحديثها.</param>
        /// <param name="dto">كائن يحمل بيانات الخامة المحدثة: الاسم، الوصف، الكمية، السعر، الصور الجديدة (اختياري).</param>
        /// <returns>كائن <c>BaseResponse&lt;ListingDto&gt;</c> يحتوي على الخامة بعد التحديث.</returns>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateListingDto dto)
        {
            var response = await _listingManager.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// نشر خامة لجعلها متاحة في السوق العام.
        /// </summary>
        /// <remarks>
        /// هذه النقطة محمية وتتطلب تفويضاً <c>[Authorize]</c>.
        /// تستخدم <c>BaseResponse&lt;ListingDto&gt;</c> لتغليف الاستجابة.
        /// تحول الخامة من حالة المسودة إلى حالة منشورة وتصبح مرئية في نقطة <c>GetPublished</c>.
        /// هذه خطوة حاسمة في دورة حياة الخامة: مسودة ← منشورة.
        /// لا يمكن نشر خامة تابعة لمصنع آخر.
        /// </remarks>
        /// <param name="id">المعرف الرقمي للخامة المراد نشرها.</param>
        /// <returns>كائن <c>BaseResponse&lt;ListingDto&gt;</c> يحتوي على الخامة بعد تغيير حالتها إلى منشورة.</returns>
        [HttpPost("{id:int}/publish")]
        [Authorize]
        public async Task<IActionResult> Publish(int id)
        {
            var response = await _listingManager.PublishAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// حذف خامة (حذف منطقي مع تنظيف الصور من Cloudinary).
        /// </summary>
        /// <remarks>
        /// هذه النقطة محمية وتتطلب تفويضاً <c>[Authorize]</c>.
        /// تستخدم <c>BaseResponse&lt;string&gt;</c> لتغليف الاستجابة.
        /// يتم الحذف بشكل منطقي (Soft Delete) مع الاحتفاظ بالسجل في قاعدة البيانات.
        /// يتم تنظيف الصور المرتبطة بالخامة من خدمة Cloudinary تلقائياً.
        /// لا يمكن حذف خامة تابعة لمصنع آخر.
        /// </remarks>
        /// <param name="id">المعرف الرقمي للخامة المراد حذفها.</param>
        /// <returns>كائن <c>BaseResponse&lt;string&gt;</c> يحمل رسالة تأكيد نجاح الحذف.</returns>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _listingManager.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
using BLL.DTOs.Common;
using BLL.DTOs.Listings;
using BLL.Managers;
using BLL.ServiceExtension; // ضفنا دي عشان يشوف فولدر الـ Services بتاعكم
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Fayed_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingManager _listingManager;
        private readonly IValidator<CreateListingDto> _createValidator;
        private readonly IAiSearchService _aiSearchService; // تعريف خدمة الذكاء الاصطناعي

        public ListingsController(
            IListingManager listingManager,
            IValidator<CreateListingDto> createValidator,
            IAiSearchService aiSearchService) // حقن الخدمة هنا
        {
            _listingManager = listingManager;
            _createValidator = createValidator;
            _aiSearchService = aiSearchService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ListingDto>>> GetPublished(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? materialType = null)
            => Ok(await _listingManager.GetPublishedAsync(page, pageSize, materialType));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ListingDetailsDto>> GetById(int id)
        {
            var listing = await _listingManager.GetByIdAsync(id);
            return listing is null ? NotFound() : Ok(listing);
        }

        [HttpPost]
        public async Task<ActionResult<ListingDetailsDto>> Create(CreateListingDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var modelState = new ModelStateDictionary();
                foreach (var error in validation.Errors)
                    modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return ValidationProblem(modelState);
            }

            var created = await _listingManager.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateListingDto dto)
            => await _listingManager.UpdateAsync(id, dto) ? NoContent() : NotFound();

        [HttpPost("{id:int}/publish")]
        public async Task<IActionResult> Publish(int id)
            => await _listingManager.PublishAsync(id) ? NoContent() : NotFound();

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
            => await _listingManager.DeleteAsync(id) ? NoContent() : NotFound();

        // ========================================================
        // ( Endpoints الخاصة بالبحث الجديد (عادي + ذكي)
        // ========================================================

        // 1. البحث العادي بالفلاتر (القايمة اللي ع اليمين)
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<ListingDto>>> SearchListings([FromQuery] ListingSearchParametersDto searchParams)
        {
            var result = await _listingManager.SearchListingsAsync(searchParams);
            return Ok(result);
        }

        // 2. البحث الذكي باستخدام الذكاء الاصطناعي (مربع البحث الذكي اللي فوق)
        [HttpPost("smart-search")]
        public async Task<ActionResult<PagedResult<ListingDto>>> SmartSearch([FromBody] SmartSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("يجب إدخال نص للبحث");

            // الـ AI بياخد الكلام يحلله ويحوله لـ DTO مليان فلاتر
            var aiFilters = await _aiSearchService.ParseSearchQueryAsync(request.Query);

            // بناخد الفلاتر اللي طلعت ونبعتها للـ Manager اللي بيكلم الداتا بيز
            var result = await _listingManager.SearchListingsAsync(aiFilters);

            return Ok(result);
        }
    }

    // كلاس خفيف ومؤقت عشان نستقبل فيه جملة البحث من الفرونت إند
    public class SmartSearchRequest
    {
        public string Query { get; set; } = string.Empty;
    }
}
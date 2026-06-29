using BLL.DTOs.Common;
using BLL.DTOs.Listings;
using BLL.Managers;
using BLL.Managers.AiManager;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fayed_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingManager _listingManager;
        private readonly IValidator<CreateListingDto> _createValidator;
        private readonly ISmartSearchManager _smartSearchManager; // AI-powered smart search (+ logging)

        public ListingsController(
            IListingManager listingManager,
            IValidator<CreateListingDto> createValidator,
            ISmartSearchManager smartSearchManager)
        {
            _listingManager = listingManager;
            _createValidator = createValidator;
            _smartSearchManager = smartSearchManager;
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
        //  Search endpoints (filtered + AI smart search)
        // ========================================================

        // 1. Filtered search (the side filter panel).
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<ListingDto>>> SearchListings([FromQuery] ListingSearchParametersDto searchParams)
        {
            var result = await _listingManager.SearchListingsAsync(searchParams);
            return Ok(result);
        }

        // 2. AI smart search (the free-text box). Parses the prompt into filters, searches,
        //    and logs the attempt to AISearchLogs.
        [HttpPost("smart-search")]
        public async Task<ActionResult<PagedResult<ListingDto>>> SmartSearch([FromBody] SmartSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("A search prompt is required.");

            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)
                ? uid
                : (int?)null;

            var result = await _smartSearchManager.SmartSearchAsync(request.Query, userId);
            return Ok(result);
        }
    }

    // Lightweight request body for the smart-search prompt coming from the front end.
    public class SmartSearchRequest
    {
        public string Query { get; set; } = string.Empty;
    }
}

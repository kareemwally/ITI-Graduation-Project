using BLL.DTOs.Common;
using BLL.DTOs.Listings;
using BLL.Managers;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fayed_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingManager _listingManager;
        private readonly IValidator<CreateListingDto> _createValidator;

        public ListingsController(
            IListingManager listingManager,
            IValidator<CreateListingDto> createValidator)
        {
            _listingManager = listingManager;
            _createValidator = createValidator;
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
    }
}

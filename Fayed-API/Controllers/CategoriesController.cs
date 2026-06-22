using BLL.DTOs.Categories;
using BLL.Managers;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fayed_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryManager _categoryManager;
        private readonly IValidator<CreateCategoryDto> _createValidator;

        public CategoriesController(
            ICategoryManager categoryManager,
            IValidator<CreateCategoryDto> createValidator)
        {
            _categoryManager = categoryManager;
            _createValidator = createValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll()
            => Ok(await _categoryManager.GetAllAsync());

        [HttpGet("tree")]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetTree()
            => Ok(await _categoryManager.GetTreeAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var category = await _categoryManager.GetByIdAsync(id);
            return category is null ? NotFound() : Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return ValidationProblem(ToModelState(validation));

            var created = await _categoryManager.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
            => await _categoryManager.UpdateAsync(id, dto) ? NoContent() : NotFound();

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
            => await _categoryManager.DeleteAsync(id) ? NoContent() : NotFound();

        private static ModelStateDictionary ToModelState(FluentValidation.Results.ValidationResult result)
        {
            var modelState = new ModelStateDictionary();
            foreach (var error in result.Errors)
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return modelState;
        }
    }
}

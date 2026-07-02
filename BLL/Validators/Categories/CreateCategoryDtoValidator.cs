using BLL.DTOs.Categories;
using FluentValidation;

namespace BLL.Validators.Categories
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم الفئة مطلوب.")
                .MaximumLength(100).WithMessage("اسم الفئة لا يتجاوز 100 حرف.");

            RuleFor(x => x.ParentId)
                .GreaterThan(0).When(x => x.ParentId.HasValue)
                .WithMessage("معرف الفئة الأصلية يجب أن يكون رقمًا موجبًا عند توفيره.");
        }
    }
}

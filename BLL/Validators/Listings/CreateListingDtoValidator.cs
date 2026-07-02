using BLL.DTOs.Listings;
using FluentValidation;

namespace BLL.Validators.Listings
{
    public class CreateListingDtoValidator : AbstractValidator<CreateListingDto>
    {
        public CreateListingDtoValidator()
        {
            RuleFor(x => x.FactoryId)
                .GreaterThan(0).WithMessage("معرف المصنع يجب أن يكون رقمًا موجبًا.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("العنوان مطلوب.")
                .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("الوصف مطلوب.");

            RuleFor(x => x.MaterialType)
                .NotEmpty().WithMessage("نوع المادة مطلوب.")
                .MaximumLength(100).WithMessage("نوع المادة لا يتجاوز 100 حرف.");

            RuleFor(x => x.MeasureUnit)
                .NotEmpty().WithMessage("وحدة القياس مطلوبة.")
                .MaximumLength(20).WithMessage("وحدة القياس لا تتجاوز 20 حرف.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");

            RuleFor(x => x.MinPrice)
                .GreaterThan(0).WithMessage("الحد الأدنى للسعر يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.MaxPrice)
                .GreaterThan(0).WithMessage("الحد الأقصى للسعر يجب أن يكون أكبر من صفر.")
                .Must((dto, maxPrice) => maxPrice >= dto.MinPrice)
                .WithMessage("الحد الأقصى للسعر يجب أن يكون أكبر من أو يساوي الحد الأدنى.");

            RuleFor(x => x.MinOrderQuantity)
                .GreaterThan(0).WithMessage("الحد الأدنى لكمية الطلب يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(x => x.Quantity)
                .WithMessage("الحد الأدنى لكمية الطلب لا يتجاوز الكمية المتاحة.");

            RuleFor(x => x.MaterialCondition).IsInEnum().WithMessage("حالة المادة غير صالحة.");
            RuleFor(x => x.DeliveryType).IsInEnum().WithMessage("نوع التوصيل غير صالح.");
            RuleFor(x => x.PreferPayMethod).IsInEnum().WithMessage("طريقة الدفع المفضلة غير صالحة.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("تاريخ الانتهاء يجب أن يكون في المستقبل.");

            RuleFor(x => x)
                .Must(x => x.CategoryId.HasValue || !string.IsNullOrWhiteSpace(x.CustomCatName))
                .WithMessage("يرجى تقديم معرف الفئة أو اسم الفئة المخصصة.");
        }
    }
}

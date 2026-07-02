using BLL.DTOs.Listings;
using FluentValidation;

namespace BLL.Validators.Listings
{
    public class UpdateListingDtoValidator : AbstractValidator<UpdateListingDto>
    {
        public UpdateListingDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("العنوان مطلوب.")
                .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("الوصف مطلوب.")
                .MaximumLength(2000).WithMessage("الوصف لا يتجاوز 2000 حرف.");

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
                .GreaterThan(0).WithMessage("الحد الأقصى للسعر يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.MinOrderQuantity)
                .GreaterThan(0).WithMessage("الحد الأدنى لكمية الطلب يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.MaterialCondition)
                .IsInEnum().WithMessage("حالة المادة غير صالحة.");

            RuleFor(x => x.DeliveryType)
                .IsInEnum().WithMessage("نوع التوصيل غير صالح.");

            RuleFor(x => x.PreferPayMethod)
                .IsInEnum().WithMessage("طريقة الدفع المفضلة غير صالحة.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("تاريخ الانتهاء يجب أن يكون في المستقبل.");
        }
    }
}

using BLL.DTOs.Common;
using FluentValidation;

namespace BLL.Validators.Common
{
    public class PublishedListingsFilterDtoValidator : AbstractValidator<PublishedListingsFilterDto>
    {
        public PublishedListingsFilterDtoValidator()
        {
            RuleFor(x => x.MinQuantity)
                .GreaterThan(0).When(x => x.MinQuantity.HasValue)
                .WithMessage("الحد الأدنى للكمية يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.MaxQuantity)
                .GreaterThan(0).When(x => x.MaxQuantity.HasValue)
                .WithMessage("الحد الأقصى للكمية يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.MinPrice)
                .GreaterThan(0).When(x => x.MinPrice.HasValue)
                .WithMessage("الحد الأدنى للسعر يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.MaxPrice)
                .GreaterThan(0).When(x => x.MaxPrice.HasValue)
                .WithMessage("الحد الأقصى للسعر يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).When(x => x.CategoryId.HasValue)
                .WithMessage("معرف الفئة يجب أن يكون رقمًا موجبًا.");

            RuleFor(x => x.SortDirection)
                .Must(d => d == "asc" || d == "desc")
                .When(x => !string.IsNullOrEmpty(x.SortDirection))
                .WithMessage("اتجاه الترتيب يجب أن يكون 'asc' أو 'desc'.");
        }
    }
}

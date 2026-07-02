using BLL.DTOs.Orders;
using FluentValidation;

namespace BLL.Validators.Orders
{
    public class SubmitMarketOfferDtoValidator : AbstractValidator<SubmitMarketOfferDto>
    {
        public SubmitMarketOfferDtoValidator()
        {
            RuleFor(x => x.ListingId)
                .GreaterThan(0).WithMessage("معرف الإعلان يجب أن يكون رقمًا موجبًا.");

            RuleFor(x => x.PricePerUnit)
                .GreaterThan(0).WithMessage("السعر لكل وحدة يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");
        }
    }
}

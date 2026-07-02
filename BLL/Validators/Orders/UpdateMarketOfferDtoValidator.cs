using BLL.DTOs.UserDashboard;
using FluentValidation;

namespace BLL.Validators.Orders
{
    public class UpdateMarketOfferDtoValidator : AbstractValidator<UpdateMarketOfferDto>
    {
        public UpdateMarketOfferDtoValidator()
        {
            RuleFor(x => x.NewQuantity)
                .GreaterThan(0).WithMessage("الكمية الجديدة يجب أن تكون أكبر من صفر.");

            RuleFor(x => x.NewPricePerUnit)
                .GreaterThan(0).WithMessage("السعر الجديد لكل وحدة يجب أن يكون أكبر من صفر.");
        }
    }
}

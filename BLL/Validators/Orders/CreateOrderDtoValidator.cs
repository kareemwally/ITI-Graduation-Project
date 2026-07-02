using BLL.DTOs.Orders;
using FluentValidation;

namespace BLL.Validators.Orders
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.ListingId)
                .GreaterThan(0).WithMessage("معرف الإعلان يجب أن يكون رقمًا موجبًا.");

            RuleFor(x => x.BuyerId)
                .GreaterThan(0).WithMessage("معرف المشتري يجب أن يكون رقمًا موجبًا.");

            RuleFor(x => x.SellerId)
                .GreaterThan(0).WithMessage("معرف البائع يجب أن يكون رقمًا موجبًا.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");
        }
    }
}

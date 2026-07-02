using BLL.DTOs.Contracts;
using FluentValidation;

namespace BLL.Validators.Contracts
{
    public class SubmitContractDtoValidator : AbstractValidator<SubmitContractDto>
    {
        public SubmitContractDtoValidator()
        {
            RuleFor(x => x.AgreedQuantity)
                .GreaterThan(0).WithMessage("الكمية المتفق عليها يجب أن تكون أكبر من صفر.");

            RuleFor(x => x.AgreedPricePerUnit)
                .GreaterThan(0).WithMessage("السعر المتفق عليه لكل وحدة يجب أن يكون أكبر من صفر.");

            RuleFor(x => x.DeliveryDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("تاريخ التسليم يجب أن يكون في المستقبل.");

            RuleFor(x => x.DeliveryAddress)
                .NotEmpty().WithMessage("عنوان التسليم مطلوب.")
                .MaximumLength(500).WithMessage("عنوان التسليم لا يتجاوز 500 حرف.");
        }
    }
}

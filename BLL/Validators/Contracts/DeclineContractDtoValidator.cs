using BLL.DTOs.Contracts;
using FluentValidation;

namespace BLL.Validators.Contracts
{
    public class DeclineContractDtoValidator : AbstractValidator<DeclineContractDto>
    {
        public DeclineContractDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("سبب الرفض مطلوب.")
                .MaximumLength(500).WithMessage("سبب الرفض لا يتجاوز 500 حرف.");
        }
    }
}

using BLL.DTOs.Disputes;
using FluentValidation;

namespace BLL.Validators.Disputes
{
    public class CreateDisputeDtoValidator : AbstractValidator<CreateDisputeDto>
    {
        public CreateDisputeDtoValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("معرف الطلب يجب أن يكون رقمًا موجبًا.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("العنوان مطلوب.")
                .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("الوصف مطلوب.")
                .MaximumLength(2000).WithMessage("الوصف لا يتجاوز 2000 حرف.");
        }
    }
}

using BLL.DTOs.Profile;
using FluentValidation;

namespace BLL.Validators.Profile
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).When(x => x.Name != null)
                .WithMessage("الاسم لا يتجاوز 100 حرف.");

            RuleFor(x => x.Address)
                .MaximumLength(200).When(x => x.Address != null)
                .WithMessage("العنوان لا يتجاوز 200 حرف.");

            RuleFor(x => x.Sector)
                .MaximumLength(100).When(x => x.Sector != null)
                .WithMessage("القطاع لا يتجاوز 100 حرف.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).When(x => x.PhoneNumber != null)
                .WithMessage("رقم الهاتف لا يتجاوز 20 رقمًا.");
        }
    }
}

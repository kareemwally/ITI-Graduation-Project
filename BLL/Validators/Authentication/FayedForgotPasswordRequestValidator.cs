using BLL.DTOs.Authentication;
using FluentValidation;

namespace BLL.Validators.Authentication
{
    public class FayedForgotPasswordRequestValidator : AbstractValidator<FayedForgotPasswordRequest>
    {
        public FayedForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صالح.");
        }
    }
}

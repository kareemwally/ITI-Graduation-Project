using BLL.DTOs.Authentication;
using FluentValidation;

namespace BLL.Validators.Authentication
{
    public class FayedLoginRequestValidator : AbstractValidator<FayedLoginRequest>
    {
        public FayedLoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صالح.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.");
        }
    }
}

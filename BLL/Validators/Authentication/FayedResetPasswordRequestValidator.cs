using BLL.DTOs.Authentication;
using FluentValidation;

namespace BLL.Validators.Authentication
{
    public class FayedResetPasswordRequestValidator : AbstractValidator<FayedResetPasswordRequest>
    {
        public FayedResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صالح.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("الرمز مطلوب.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.");
        }
    }
}

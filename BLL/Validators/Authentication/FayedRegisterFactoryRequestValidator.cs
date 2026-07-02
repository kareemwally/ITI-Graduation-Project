using BLL.DTOs.Authentication;
using FluentValidation;

namespace BLL.Validators.Authentication
{
    public class FayedRegisterFactoryRequestValidator : AbstractValidator<FayedRegisterFactoryRequest>
    {
        public FayedRegisterFactoryRequestValidator()
        {
            RuleFor(x => x.FactoryName)
                .NotEmpty().WithMessage("اسم المصنع مطلوب.")
                .MaximumLength(200).WithMessage("اسم المصنع لا يتجاوز 200 حرف.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صالح.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("الاسم مطلوب.")
                .MaximumLength(100).WithMessage("الاسم لا يتجاوز 100 حرف.");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرقم القومي مطلوب.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("العنوان مطلوب.")
                .MaximumLength(500).WithMessage("العنوان لا يتجاوز 500 حرف.");

            RuleFor(x => x.Sector)
                .NotEmpty().WithMessage("القطاع مطلوب.");

            RuleFor(x => x.CommercialRegistryNo)
                .NotEmpty().WithMessage("رقم السجل التجاري مطلوب.");

            RuleFor(x => x.TaxCardNo)
                .NotEmpty().WithMessage("رقم البطاقة الضريبية مطلوب.");

            RuleFor(x => x.CommercialRegistryFile)
                .NotNull().WithMessage("ملف السجل التجاري مطلوب.");

            RuleFor(x => x.TaxCardFile)
                .NotNull().WithMessage("ملف البطاقة الضريبية مطلوب.");

            RuleFor(x => x.NationalIdFile)
                .NotNull().WithMessage("ملف الرقم القومي مطلوب.");

            RuleFor(x => x.SelfieWithIdFile)
                .NotNull().WithMessage("صورة السيلفي مع الهوية مطلوبة.");
        }
    }
}

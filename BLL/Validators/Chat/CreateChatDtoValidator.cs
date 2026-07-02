using BLL.DTOs.Chat;
using FluentValidation;

namespace BLL.Validators.Chat
{
    public class CreateChatDtoValidator : AbstractValidator<CreateChatDto>
    {
        public CreateChatDtoValidator()
        {
            RuleFor(x => x.ListingId)
                .GreaterThan(0).WithMessage("معرف الإعلان يجب أن يكون رقمًا موجبًا.");
        }
    }
}

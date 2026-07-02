using BLL.DTOs.Chat;
using FluentValidation;

namespace BLL.Validators.Chat
{
    public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
    {
        public SendMessageDtoValidator()
        {
            RuleFor(x => x.Content)
                .MaximumLength(1000).WithMessage("محتوى الرسالة لا يتجاوز 1000 حرف.");

            RuleFor(x => x.MessageType)
                .IsInEnum().WithMessage("نوع الرسالة غير صالح.");
        }
    }
}

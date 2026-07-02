using System.Net;

namespace DAL.Models.ExceptionModels
{
    public class ChatNotFoundException : BaseBusinessException
    {
        public ChatNotFoundException(int chatId)
            : base($"المحادثة رقم {chatId} غير موجودة.", HttpStatusCode.NotFound)
        {
        }
    }
}

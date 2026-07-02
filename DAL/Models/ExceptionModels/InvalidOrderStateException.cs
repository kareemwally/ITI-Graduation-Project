using System.Net;

namespace DAL.Models.ExceptionModels
{
    public class InvalidOrderStateException : BaseBusinessException
    {
        public InvalidOrderStateException(string message)
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}

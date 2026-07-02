using System.Net;

namespace DAL.Models.ExceptionModels
{
    public class OrderNotFoundException : BaseBusinessException
    {
        public OrderNotFoundException(int orderId)
            : base($"الطلب رقم {orderId} غير موجود.", HttpStatusCode.NotFound)
        {
        }
    }
}

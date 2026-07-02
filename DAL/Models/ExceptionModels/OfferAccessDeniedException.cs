using System.Net;

namespace DAL.Models.ExceptionModels
{
    public class OfferAccessDeniedException : BaseBusinessException
    {
        public OfferAccessDeniedException()
            : base("ليس لديك صلاحية الوصول إلى هذا العرض.", HttpStatusCode.Forbidden)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class UnverifiedFactoryException : BaseBusinessException
    {
        public UnverifiedFactoryException(
            string message = "عذراً، يجب توثيق حساب المصنع أولاً لتتمكن من إتمام هذا الإجراء.")
            : base(message , HttpStatusCode.Forbidden, errors: null)
        {
        }
    }
}

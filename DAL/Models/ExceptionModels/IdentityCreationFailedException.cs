using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class IdentityCreationFailedException : BaseBusinessException
    {
        public IdentityCreationFailedException(List<string> errors)
              : base("فشل إنشاء الحساب بسبب عدم استيفاء شروط الأمان.", HttpStatusCode.BadRequest, errors) { }
    }
}

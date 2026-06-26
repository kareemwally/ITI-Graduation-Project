using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class InvalidTokenException : BaseBusinessException
    {
        public InvalidTokenException()
            : base("رابط تفعيل الحساب منتهي الصلاحية أو غير صالح.", HttpStatusCode.BadRequest) { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class EmailAlreadyExistsException : BaseBusinessException
    {
        public EmailAlreadyExistsException()
            : base("هذا البريد الإلكتروني مسجل بالفعل في النظام.", HttpStatusCode.BadRequest) { }
    }
}

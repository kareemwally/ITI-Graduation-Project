using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class UserNotFoundException : BaseBusinessException
    {
        public UserNotFoundException()
            : base("عذراً، هذا المستخدم غير مسجل لدينا.", HttpStatusCode.NotFound) { } 
    }
}

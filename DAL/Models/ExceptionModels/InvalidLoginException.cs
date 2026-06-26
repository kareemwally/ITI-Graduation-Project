using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class InvalidLoginException : BaseBusinessException
    {
        public InvalidLoginException() : 
            base("There is something wrong with your Email or Password",HttpStatusCode.BadRequest) 
        {

        }
    }
}

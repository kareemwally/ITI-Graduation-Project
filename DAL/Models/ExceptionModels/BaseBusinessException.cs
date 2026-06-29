using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public abstract class BaseBusinessException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public List<string>? Errors { get; }
        
        protected BaseBusinessException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}

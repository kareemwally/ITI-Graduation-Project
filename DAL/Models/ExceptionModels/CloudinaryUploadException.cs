using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class CloudinaryUploadException : BaseBusinessException
    {
        public CloudinaryUploadException(string message) : base(message)
        {
        }
    }
}

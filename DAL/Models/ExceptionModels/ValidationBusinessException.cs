using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    using System.Collections.Generic;
    using System.Net;

    namespace DAL.Models.ExceptionModels
    {
        /// <summary>
        /// إكسبشن مخصص لمعالجة أخطاء الـ FluentValidation في طبقة الـ BLL
        /// </summary>
        public class ValidationBusinessException : BaseBusinessException
        {
            // 1. كونسرتكتور يقبل لستة الأخطاء كاملة مع رسالة عامة ثابتة
            public ValidationBusinessException(List<string> errors)
                : base("عذراً، البيانات المرسلة غير صالحة.", HttpStatusCode.BadRequest, errors)
            {
            }

            // 2. كونسرتكتور إضافي لو حابب تبعت رسالة مخصصة مع الأخطاء
            public ValidationBusinessException(string message, List<string> errors)
                : base(message, HttpStatusCode.BadRequest, errors)
            {
            }
        }
    }
}

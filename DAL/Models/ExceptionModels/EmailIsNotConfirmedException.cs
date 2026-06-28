using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.ExceptionModels
{
    public class EmailIsNotConfirmedException : BaseBusinessException
    {
        public EmailIsNotConfirmedException():
            base("هذا الحساب لم يتم توثيقه بعد الرجاء تتبع صندوق الرسائل لحسابك الذي سجلت به والبحث عن بريد تأكيد الحساب الخاص بمنصتنا")
        {

        }
    }
}

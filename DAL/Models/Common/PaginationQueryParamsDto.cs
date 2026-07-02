using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models.Common
{
    public class PaginationQueryParamsDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12; // القيمة الافتراضية لصفحة المخازن
    }
}

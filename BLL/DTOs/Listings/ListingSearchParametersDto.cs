using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Listings
{
    public class ListingSearchParametersDto
    {
        public string? SearchTerm { set; get; }
        public int? CategoryId { get; set; }
        /// <summary>Free-text location to match against the factory's address (e.g. "Cairo" / "القاهرة").</summary>
        public string? Location { get; set; }
        public decimal?MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;  //  الصفحة تقريبا بيظهر فيها العدد ده . 



    }
}

using BLL.DTOs.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ServiceExtension
{
     public interface IAiSearchService
    {
        Task<ListingSearchParametersDto> ParseSearchQueryAsync(string userQuery);
    }
}

using BLL.DTOs.Common;
using BLL.DTOs.Listings;

namespace BLL.Managers.AiManager
{
    /// <summary>
    /// Orchestrates the AI-powered smart search: parse the prompt into filters, run the search,
    /// and record the attempt in <c>AISearchLogs</c> for analytics.
    /// </summary>
    public interface ISmartSearchManager
    {
        Task<PagedResult<ListingDto>> SmartSearchAsync(string query, int? userId);
    }
}

using System.Text.Json;
using BLL.AI.Abstractions;
using BLL.DTOs.Common;
using BLL.DTOs.Listings;
using BLL.ServiceExtension;
using DAL.Models;
using DAL.UnitOfWork;

namespace BLL.Managers.AiManager
{
    public class SmartSearchManager : ISmartSearchManager
    {
        private readonly IAiSearchService _aiSearchService;
        private readonly IListingManager _listingManager;
        private readonly IAiProviderResolver _providerResolver;
        private readonly IUnitOfWork _uow;

        public SmartSearchManager(
            IAiSearchService aiSearchService,
            IListingManager listingManager,
            IAiProviderResolver providerResolver,
            IUnitOfWork uow)
        {
            _aiSearchService = aiSearchService;
            _listingManager = listingManager;
            _providerResolver = providerResolver;
            _uow = uow;
        }

        public async Task<PagedResult<ListingDto>> SmartSearchAsync(string query, int? userId)
        {
            // 1. Let the AI translate the natural-language prompt into structured filters.
            var filters = await _aiSearchService.ParseSearchQueryAsync(query);

            // 2. Execute the actual search against the database.
            var results = await _listingManager.SearchListingsAsync(filters);

            // 3. Record the search for analytics (never let logging break the user's request).
            await TryLogAsync(query, filters, results, userId);

            return results;
        }

        private async Task TryLogAsync(
            string query, ListingSearchParametersDto filters, PagedResult<ListingDto> results, int? userId)
        {
            try
            {
                var topListingIds = results.Items.Take(10).Select(l => l.Id).ToList();

                var log = new AISearchLog
                {
                    UserId = userId,
                    PromptText = query,
                    ExtractedFilters = JsonSerializer.Serialize(filters),
                    ResultsCount = results.TotalCount,
                    TopListingIds = JsonSerializer.Serialize(topListingIds),
                    ModelVersion = _providerResolver.GetActiveProvider().ModelVersion,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Repository<AISearchLog>().AddAsync(log);
                await _uow.SaveChangesAsync();
            }
            catch
            {
                // Analytics logging is best-effort; swallow failures.
            }
        }
    }
}

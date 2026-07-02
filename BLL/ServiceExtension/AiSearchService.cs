using System.Text.Json;
using BLL.AI;
using BLL.AI.Abstractions;
using BLL.DTOs.Listings;

namespace BLL.ServiceExtension
{
    /// <summary>
    /// Turns a free-text (Arabic or English) search prompt into structured listing filters.
    /// It depends only on <see cref="IAiProviderResolver"/>, so the underlying model (Gemini,
    /// Claude, ...) can be swapped from configuration without touching this code.
    /// </summary>
    public class AiSearchService : IAiSearchService
    {
        private readonly IAiProviderResolver _providerResolver;

        public AiSearchService(IAiProviderResolver providerResolver)
        {
            _providerResolver = providerResolver;
        }

        public async Task<ListingSearchParametersDto> ParseSearchQueryAsync(string userQuery)
        {
            const string systemPrompt = @"You are a smart search assistant for a B2B Industrial Symbiosis marketplace.
Your strict task is to extract search parameters from the user's input (which may be in Arabic or English) and return them EXCLUSIVELY as a raw, valid JSON object.
DO NOT wrap the response in markdown blocks. DO NOT add any conversational text or explanations.
If a specific filter is not mentioned in the user's request, set its value to null.

Here is the EXACT JSON structure you must return:
{
   ""MinQuantity"": null,
   ""MaxQuantity"": null,
   ""MinPrice"": null,
   ""MaxPrice"": null,
   ""CategoryId"": null,
   ""Location"": null
}

Guidelines:
- CategoryId (map text to these exact IDs): Plastic/بلاستيك=1, Metal/Iron/معادن/حديد=2, Paper/Carton/ورق/كرتون=3.
- Location: extract the place/city/governorate name mentioned (Arabic or English) as plain text, e.g. ""Cairo"" or ""القاهرة"". Do NOT convert it to an ID.

Return ONLY the JSON object.";

            var provider = _providerResolver.GetActiveProvider();

            ListingSearchParametersDto? searchParams = null;
            try
            {
                var raw = await provider.CompleteAsync(new AiCompletionRequest
                {
                    SystemPrompt = systemPrompt,
                    UserPrompt = $"User Request: {userQuery}",
                    ExpectJson = true,
                    MaxOutputTokens = 512
                });

                searchParams = JsonSerializer.Deserialize<ListingSearchParametersDto>(
                    AiJson.Clean(raw),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // On any AI/parse failure, fall back to an empty filter set so search still works
                // (it simply returns the latest listings) instead of failing the request.
                searchParams = null;
            }

            searchParams ??= new ListingSearchParametersDto();
            searchParams.SearchTerm = userQuery;
            return searchParams;
        }
    }
}

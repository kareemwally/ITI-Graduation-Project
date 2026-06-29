using BLL.DTOs.Listings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.ServiceExtension
{
    public class AiSearchService:IAiSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AiSearchService(HttpClient httpClient , IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini API Key is missing");

        }
        public async Task<ListingSearchParametersDto> ParseSearchQueryAsync(string userQuery)
        {
            // 1. Bulletproof English Prompt
            string systemPrompt = @"You are a smart search assistant for a B2B Industrial Symbiosis marketplace.
Your strict task is to extract search parameters from the user's input (which may be in Arabic or English) and return them EXCLUSIVELY as a raw, valid JSON object. 
DO NOT wrap the response in markdown blocks (e.g., no ```json). DO NOT add any conversational text or explanations.
If a specific filter is not mentioned in the user's request, set its value to null.

Here is the EXACT JSON structure you must return:
{
   ""MinQuantity"": null,
   ""MaxQuantity"": null,
   ""MinPrice"": null,
   ""MaxPrice"": null,
   ""CategoryId"": null,
   ""GovernorateId"": null
}

Guidelines for mapping text to IDs (use these exact IDs if you detect the corresponding meaning):
- CategoryId: Plastic/بلاستيك=1, Metal/Iron/معادن/حديد=2, Paper/Carton/ورق/كرتون=3.
- GovernorateId: Cairo/القاهرة=1, Alexandria/الإسكندرية=2, Dakahlia/Mansoura/الدقهلية/المنصورة=3.

Return ONLY the JSON object.";

            string fullPrompt = $"{systemPrompt}\n\nUser Request: {userQuery}";

            // 2. بنجهز الريكويست اللي رايح لجوجل (Gemini 1.5 Flash السريع)
            var requestBody = new
            {
                contents = new[]
                {
            new { parts = new[] { new { text = fullPrompt } } }
        }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // 3. بنكلم الـ API
            string url = $"[https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=](https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=){_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return new ListingSearchParametersDto(); // لو حصل إيرور نرجع أوبجكت فاضي عشان السيرفر ميضربش

            // 4. بنقرأ الرد ونطلع منه الـ JSON
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseString);

            var aiTextResponse = jsonDocument.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();

            // 5. بننضف الرد لزيادة التأكيد (عشان لو الموديل عاند وبعت markdown)
            aiTextResponse = aiTextResponse?.Replace("```json", "").Replace("```", "").Trim();

            // 6. بنحول الـ JSON لكلاس الـ DTO بتاعنا
            var searchParams = JsonSerializer.Deserialize<ListingSearchParametersDto>(
                aiTextResponse ?? "{}",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (searchParams != null)
                searchParams.SearchTerm = userQuery;

            return searchParams ?? new ListingSearchParametersDto();
        }

    }
}

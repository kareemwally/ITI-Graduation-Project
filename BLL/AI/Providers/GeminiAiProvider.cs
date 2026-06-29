using System.Text;
using System.Text.Json;
using BLL.AI.Abstractions;
using Microsoft.Extensions.Configuration;

namespace BLL.AI.Providers
{
    /// <summary>
    /// Google Gemini implementation of <see cref="IAiProvider"/> (generateContent REST API).
    /// </summary>
    public class GeminiAiProvider : IAiProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _apiKey;
        private readonly string _model;

        public GeminiAiProvider(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            // Prefer the new "Ai:Gemini:*" section, fall back to the legacy "Gemini:ApiKey".
            _apiKey = config["Ai:Gemini:ApiKey"] ?? config["Gemini:ApiKey"];
            _model = config["Ai:Gemini:Model"] ?? "gemini-1.5-flash";
        }

        public string Name => "Gemini";
        public string ModelVersion => _model;

        public async Task<string> CompleteAsync(AiCompletionRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Gemini API key is not configured (Ai:Gemini:ApiKey).");

            // Build the multimodal "parts" array: the user prompt plus any inline document data.
            var parts = new List<object> { new { text = request.UserPrompt } };
            foreach (var attachment in request.Attachments)
            {
                parts.Add(new
                {
                    inline_data = new { mime_type = attachment.MimeType, data = attachment.Base64Data }
                });
            }

            var body = new
            {
                system_instruction = new { parts = new[] { new { text = request.SystemPrompt } } },
                contents = new[] { new { role = "user", parts } },
                generationConfig = new
                {
                    maxOutputTokens = request.MaxOutputTokens,
                    responseMimeType = request.ExpectJson ? "application/json" : "text/plain"
                }
            };

            var json = JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var client = _httpClientFactory.CreateClient();
            using var response = await client.PostAsync(url, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini request failed ({(int)response.StatusCode}): {responseString}");

            using var document = JsonDocument.Parse(responseString);
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }
    }
}

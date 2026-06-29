using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BLL.AI.Abstractions;
using Microsoft.Extensions.Configuration;

namespace BLL.AI.Providers
{
    /// <summary>
    /// Anthropic Claude implementation of <see cref="IAiProvider"/> (Messages API).
    /// Demonstrates that the provider is swappable purely through configuration.
    /// </summary>
    public class ClaudeAiProvider : IAiProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _apiKey;
        private readonly string _model;

        public ClaudeAiProvider(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = config["Ai:Claude:ApiKey"];
            _model = config["Ai:Claude:Model"] ?? "claude-sonnet-4-6";
        }

        public string Name => "Claude";
        public string ModelVersion => _model;

        public async Task<string> CompleteAsync(AiCompletionRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Claude API key is not configured (Ai:Claude:ApiKey).");

            // Build the user message content blocks: text first, then any document/image blocks.
            var contentBlocks = new List<object> { new { type = "text", text = request.UserPrompt } };
            foreach (var attachment in request.Attachments)
            {
                if (attachment.IsPdf)
                {
                    contentBlocks.Add(new
                    {
                        type = "document",
                        source = new { type = "base64", media_type = attachment.MimeType, data = attachment.Base64Data }
                    });
                }
                else
                {
                    contentBlocks.Add(new
                    {
                        type = "image",
                        source = new { type = "base64", media_type = attachment.MimeType, data = attachment.Base64Data }
                    });
                }
            }

            // Nudge the model to answer with raw JSON when requested.
            var system = request.ExpectJson
                ? request.SystemPrompt + "\n\nRespond with a single raw JSON object and nothing else."
                : request.SystemPrompt;

            var body = new
            {
                model = _model,
                max_tokens = request.MaxOutputTokens,
                system,
                messages = new[] { new { role = "user", content = contentBlocks } }
            };

            var json = JsonSerializer.Serialize(body);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Add("x-api-key", _apiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Claude request failed ({(int)response.StatusCode}): {responseString}");

            using var document = JsonDocument.Parse(responseString);
            var text = document.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }
    }
}

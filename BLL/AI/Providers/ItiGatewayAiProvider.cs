using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BLL.AI.Abstractions;
using Microsoft.Extensions.Configuration;

namespace BLL.AI.Providers
{
    /// <summary>
    /// Provider for the ITI student API gateway (<c>http://apiaccess.iti.net.eg/student</c>), which
    /// exposes an OpenAI-compatible Chat Completions API in front of many Bedrock models.
    /// Multimodal: document images are sent as base64 data URLs so the model can read KYB documents.
    /// </summary>
    public class ItiGatewayAiProvider : IAiProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;
        private readonly string? _apiKey;
        private readonly string _model;

        public ItiGatewayAiProvider(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _baseUrl = (config["Ai:ItiGateway:BaseUrl"] ?? "http://apiaccess.iti.net.eg/student").TrimEnd('/');
            _apiKey = config["Ai:ItiGateway:ApiKey"];
            _model = config["Ai:ItiGateway:Model"] ?? "anthropic.claude-sonnet-4-6";
        }

        public string Name => "ItiGateway";
        public string ModelVersion => _model;

        public async Task<string> CompleteAsync(AiCompletionRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("ITI gateway API key is not configured (Ai:ItiGateway:ApiKey).");

            var model = string.IsNullOrWhiteSpace(request.ModelOverride) ? _model : request.ModelOverride!;

            // OpenAI message shape. When there are attachments the user content becomes an array of
            // parts (text + image_url blocks); otherwise it is a plain string.
            object userContent;
            if (request.Attachments.Count > 0)
            {
                var parts = new List<object> { new { type = "text", text = request.UserPrompt } };
                foreach (var attachment in request.Attachments)
                {
                    parts.Add(new
                    {
                        type = "image_url",
                        image_url = new { url = $"data:{attachment.MimeType};base64,{attachment.Base64Data}" }
                    });
                }
                userContent = parts;
            }
            else
            {
                userContent = request.UserPrompt;
            }

            var body = new
            {
                model,
                messages = new object[]
                {
                    new { role = "system", content = request.SystemPrompt },
                    new { role = "user", content = userContent }
                },
                max_tokens = request.MaxOutputTokens,
                temperature = 0
            };

            var json = JsonSerializer.Serialize(body);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/chat/completions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"ITI gateway request failed ({(int)response.StatusCode}): {responseString}");

            using var document = JsonDocument.Parse(responseString);
            var text = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text ?? string.Empty;
        }
    }
}

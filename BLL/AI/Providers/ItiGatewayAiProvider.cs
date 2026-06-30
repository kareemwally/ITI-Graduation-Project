using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BLL.AI.Abstractions;
using Microsoft.Extensions.Configuration;

namespace BLL.AI.Providers
{
    /// <summary>
    /// Provider for the ITI "Student Bedrock Gateway" (<c>http://apiaccess.iti.net.eg/api/v1</c>).
    /// This is NOT an OpenAI-compatible API — it has its own shape:
    ///   - text:       POST {base}/student/chat            body { model_id, messages:[{role, content}], system_prompt, max_tokens }
    ///   - multimodal: POST {base}/student/multimodal-chat body { model_id, messages:[{role, text, images:[{format, data_base64}]}], system_prompt, max_tokens }
    /// Both return the model's answer in the top-level <c>output_text</c> field.
    /// Auth is a Bearer student key. Model ids must be on the student's allow-list (see /student/me).
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
            _baseUrl = (config["Ai:ItiGateway:BaseUrl"] ?? "http://apiaccess.iti.net.eg/api/v1").TrimEnd('/');
            _apiKey = config["Ai:ItiGateway:ApiKey"];
            _model = config["Ai:ItiGateway:Model"] ?? "qwen.qwen3-vl-235b-a22b";
        }

        public string Name => "ItiGateway";
        public string ModelVersion => _model;

        public async Task<string> CompleteAsync(AiCompletionRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("ITI gateway API key is not configured (Ai:ItiGateway:ApiKey).");

            var model = string.IsNullOrWhiteSpace(request.ModelOverride) ? _model : request.ModelOverride!;
            var hasImages = request.Attachments.Count > 0;

            // The gateway uses different routes and message shapes for text vs. multimodal.
            string path;
            object message;
            if (hasImages)
            {
                path = "/student/multimodal-chat";
                message = new
                {
                    role = "user",
                    text = request.UserPrompt,
                    images = request.Attachments.Select(a => new
                    {
                        format = ImageFormat(a.MimeType),
                        data_base64 = a.Base64Data
                    }).ToArray()
                };
            }
            else
            {
                path = "/student/chat";
                message = new { role = "user", content = request.UserPrompt };
            }

            var body = new
            {
                model_id = model,
                messages = new[] { message },
                system_prompt = request.SystemPrompt,
                max_tokens = request.MaxOutputTokens
            };

            var json = JsonSerializer.Serialize(body);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{path}")
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
            if (document.RootElement.TryGetProperty("output_text", out var output))
                return output.GetString() ?? string.Empty;

            // Defensive: surface anything unexpected rather than silently returning empty.
            throw new HttpRequestException($"ITI gateway returned no output_text: {responseString}");
        }

        // Maps a MIME type to the short format token the gateway expects (e.g. "png", "jpeg").
        private static string ImageFormat(string mimeType) => mimeType.ToLowerInvariant() switch
        {
            "image/png" => "png",
            "image/jpeg" or "image/jpg" => "jpeg",
            "image/webp" => "webp",
            "image/gif" => "gif",
            "application/pdf" => "pdf",
            _ => "png"
        };
    }
}

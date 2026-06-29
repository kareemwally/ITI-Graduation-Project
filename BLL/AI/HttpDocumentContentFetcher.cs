using BLL.AI.Abstractions;

namespace BLL.AI
{
    /// <summary>Fetches document bytes over HTTP (e.g. from Cloudinary) for multimodal analysis.</summary>
    public class HttpDocumentContentFetcher : IDocumentContentFetcher
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpDocumentContentFetcher(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AiAttachment?> FetchAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrl) || !Uri.TryCreate(fileUrl, UriKind.Absolute, out _))
                return null;

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync(fileUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                var mime = response.Content.Headers.ContentType?.MediaType ?? GuessMimeFromUrl(fileUrl);
                if (mime is null || !(mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                                      || mime.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)))
                    return null;

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return new AiAttachment
                {
                    MimeType = mime,
                    Base64Data = Convert.ToBase64String(bytes)
                };
            }
            catch
            {
                return null;
            }
        }

        private static string? GuessMimeFromUrl(string url)
        {
            var path = url.Split('?')[0];
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                _ => null
            };
        }
    }
}

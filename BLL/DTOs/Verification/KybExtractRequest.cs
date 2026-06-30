using Microsoft.AspNetCore.Http;

namespace BLL.DTOs.Verification
{
    /// <summary>
    /// multipart/form-data body for the verify-first extract step: the KYB documents to analyze.
    /// A single wrapper model so Swagger can generate the operation schema.
    /// </summary>
    public class KybExtractRequest
    {
        public List<IFormFile> Files { get; set; } = new();
    }
}

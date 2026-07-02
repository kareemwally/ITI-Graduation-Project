using Microsoft.AspNetCore.Http;

namespace BLL.DTOs.Documents
{
    /// <summary>
    /// multipart/form-data body for uploading a single factory KYB document. Wrapping the file and
    /// its type in one model (rather than separate [FromForm] parameters) keeps Swagger able to
    /// generate the operation schema.
    /// </summary>
    public class UploadFactoryDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
    }
}

using BLL.AI.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BLL.AI
{
    /// <summary>
    /// Validation rules and conversion helpers for KYB documents that are uploaded directly
    /// for AI analysis (the "verify-first" flow), before anything is stored.
    /// </summary>
    public static class KybUpload
    {
        /// <summary>Maximum accepted size per document: 5 MB.</summary>
        public const long MaxFileSizeBytes = 5L * 1024 * 1024;

        /// <summary>Extensions the AI gateway can read (images + PDF).</summary>
        public static readonly IReadOnlyCollection<string> AllowedExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".pdf" };

        /// <summary>
        /// Validates a single uploaded document. Returns <c>null</c> when the file is acceptable,
        /// otherwise a human-readable reason it was rejected.
        /// </summary>
        public static string? Validate(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return "File is empty.";

            if (file.Length > MaxFileSizeBytes)
                return $"File '{file.FileName}' is {file.Length / (1024.0 * 1024.0):0.0} MB; the maximum is 5 MB.";

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                return $"Unsupported file type '{extension}' for '{file.FileName}'. Allowed: .PNG, .JPG, .PDF.";

            return null;
        }

        /// <summary>Reads an already-validated upload into an <see cref="AiAttachment"/> for the model.</summary>
        public static async Task<AiAttachment> ToAttachmentAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            await file.CopyToAsync(memory, cancellationToken);

            return new AiAttachment
            {
                MimeType = MimeFor(file.FileName),
                Base64Data = Convert.ToBase64String(memory.ToArray())
            };
        }

        private static string MimeFor(string fileName) =>
            Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
    }
}

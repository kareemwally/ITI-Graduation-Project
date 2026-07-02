using Microsoft.AspNetCore.Http;

namespace BLL.DTOs.Verification
{
    /// <summary>
    /// multipart/form-data body of the "confirm" step. The frontend re-sends the same documents
    /// plus the AI extraction result it previously received and approved, which is then persisted
    /// alongside the stored documents.
    /// </summary>
    public class KybConfirmRequest
    {
        /// <summary>The documents to store, re-sent from the extract step.</summary>
        public List<IFormFile> Files { get; set; } = new();

        /// <summary>The JSON object of fields returned by the extract step.</summary>
        public string ExtractedFields { get; set; } = "{}";

        public decimal ConfidenceScore { get; set; }

        /// <summary>JSON array of mismatches from the extract step (nullable / optional).</summary>
        public string? Mismatches { get; set; }

        /// <summary>approve / review / reject — the AI recommendation from the extract step.</summary>
        public string? Recommendation { get; set; }

        public string? ModelVersion { get; set; }

        /// <summary>
        /// Optional document type label per uploaded file, aligned by index with the files.
        /// Missing entries default to "kyb".
        /// </summary>
        public List<string>? DocumentTypes { get; set; }
    }
}

namespace BLL.DTOs.Verification
{
    /// <summary>
    /// Body of the "confirm" step. The frontend re-sends the same documents (as multipart files)
    /// plus the AI extraction result it previously received and approved, which is then persisted
    /// alongside the stored documents.
    /// </summary>
    public class KybConfirmRequest
    {
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

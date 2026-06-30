namespace BLL.DTOs.Verification
{
    /// <summary>
    /// Result of the "verify-first" extraction step: what the AI gateway read from the uploaded
    /// KYB documents. Returned to the frontend for the user to review before anything is stored.
    /// </summary>
    public class KybExtractionResultDto
    {
        public int FactoryId { get; set; }

        /// <summary>JSON object of fields the model read from the documents.</summary>
        public string ExtractedFields { get; set; } = "{}";

        /// <summary>Model confidence in the extraction/match, 0.000 – 1.000.</summary>
        public decimal ConfidenceScore { get; set; }

        /// <summary>JSON array of detected conflicts between declared data and the documents (nullable).</summary>
        public string? Mismatches { get; set; }

        /// <summary>approve / review / reject (advisory only).</summary>
        public string Recommendation { get; set; } = null!;

        public string ModelVersion { get; set; } = null!;

        /// <summary>Echo of the files analyzed, so the frontend can re-send the same set on confirm.</summary>
        public IReadOnlyList<AnalyzedFileDto> Files { get; set; } = Array.Empty<AnalyzedFileDto>();
    }

    /// <summary>Lightweight echo of one analyzed upload (no bytes — purely informational).</summary>
    public class AnalyzedFileDto
    {
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long SizeBytes { get; set; }
    }
}

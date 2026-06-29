namespace BLL.DTOs.Verification
{
    /// <summary>API-facing view of an AI verification analysis for a factory KYB case.</summary>
    public class AiVerificationResultDto
    {
        public int Id { get; set; }
        public int VerificationCaseId { get; set; }
        public int FactoryId { get; set; }

        /// <summary>JSON object of fields the model read from the documents.</summary>
        public string ExtractedFields { get; set; } = "{}";

        /// <summary>Model confidence in the extraction/match, 0.000 – 1.000.</summary>
        public decimal ConfidenceScore { get; set; }

        /// <summary>JSON array of detected conflicts between claimed data and the documents (nullable).</summary>
        public string? Mismatches { get; set; }

        /// <summary>approve / review / reject.</summary>
        public string Recommendation { get; set; } = null!;

        public string ModelVersion { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}

using System.Text;
using System.Text.Json;
using BLL.AI;
using BLL.AI.Abstractions;
using BLL.DTOs.Common;
using BLL.DTOs.Verification;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;

namespace BLL.Managers.Verification
{
    public class VerificationManager : IVerificationManager
    {
        private const int MaxDocumentsAnalyzed = 5;

        private readonly IUnitOfWork _uow;
        private readonly IAiProviderResolver _providerResolver;
        private readonly IDocumentContentFetcher _documentFetcher;

        public VerificationManager(
            IUnitOfWork uow,
            IAiProviderResolver providerResolver,
            IDocumentContentFetcher documentFetcher)
        {
            _uow = uow;
            _providerResolver = providerResolver;
            _documentFetcher = documentFetcher;
        }

        public async Task<BaseResponse<AiVerificationResultDto>> RunAiVerificationAsync(int factoryId)
        {
            var factory = await _uow.Repository<Factory>().GetByIdAsync(factoryId);
            if (factory is null)
                return BaseResponse<AiVerificationResultDto>.Failure("Factory not found.", statusCode: 404);

            var documents = await _uow.Repository<Document>().FindAsync(d => d.FactoryId == factoryId);
            if (documents.Count == 0)
                return BaseResponse<AiVerificationResultDto>.Failure(
                    "No KYB documents found for this factory.", statusCode: 400);

            // 1. Pull document bytes (best-effort) so the model can read them.
            var attachments = new List<AiAttachment>();
            foreach (var doc in documents.Take(MaxDocumentsAnalyzed))
            {
                var attachment = await _documentFetcher.FetchAsync(doc.FileUrl);
                if (attachment is not null)
                    attachments.Add(attachment);
            }

            // 2. Ask the active AI provider to extract and cross-check the data.
            var provider = _providerResolver.GetActiveProvider();
            string raw;
            try
            {
                raw = await provider.CompleteAsync(new AiCompletionRequest
                {
                    SystemPrompt = BuildSystemPrompt(),
                    UserPrompt = BuildUserPrompt(factory, documents),
                    Attachments = attachments,
                    ExpectJson = true,
                    MaxOutputTokens = 1500
                });
            }
            catch (Exception ex)
            {
                return BaseResponse<AiVerificationResultDto>.Failure(
                    $"AI verification failed: {ex.Message}", statusCode: 502);
            }

            var analysis = ParseAnalysis(raw);

            // 3. Attach the result to a verification case (reuse the latest open one or open a new one).
            var verificationCase = await GetOrCreateOpenCaseAsync(factoryId);

            var aiResult = new AIVerificationResult
            {
                ExtractedFields = analysis.ExtractedFields,
                ConfidenceScore = analysis.ConfidenceScore,
                Mismatches = analysis.Mismatches,
                AIRecommendation = analysis.Recommendation,
                ModelVersion = provider.ModelVersion,
                CreatedAt = DateTime.UtcNow
            };

            var resultRepo = _uow.Repository<AIVerificationResult>();
            if (verificationCase.Id != 0)
            {
                // Existing case: replace any prior AI result (1..1 unique key) — "delete old, add new".
                var existing = await resultRepo.FirstOrDefaultAsync(r => r.VerificationCaseId == verificationCase.Id);
                if (existing is not null)
                    resultRepo.Remove(existing);

                aiResult.VerificationCaseId = verificationCase.Id;
                await resultRepo.AddAsync(aiResult);

                verificationCase.Status = VerificationCaseStatus.UnderReview;
                _uow.Repository<VerificationCase>().Update(verificationCase);
            }
            else
            {
                // Brand-new case: let EF insert the case and its result together.
                verificationCase.AIVerificationResult = aiResult;
                await _uow.Repository<VerificationCase>().AddAsync(verificationCase);
            }

            await _uow.SaveChangesAsync();

            return BaseResponse<AiVerificationResultDto>.Success(
                ToDto(aiResult, verificationCase.Id, factoryId),
                "AI verification completed.");
        }

        public async Task<BaseResponse<AiVerificationResultDto>> GetResultByCaseAsync(int verificationCaseId)
        {
            var result = await _uow.Repository<AIVerificationResult>()
                .FirstOrDefaultAsync(r => r.VerificationCaseId == verificationCaseId);

            if (result is null)
                return BaseResponse<AiVerificationResultDto>.Failure(
                    "No AI verification result for this case.", statusCode: 404);

            var verificationCase = await _uow.Repository<VerificationCase>().GetByIdAsync(verificationCaseId);
            var factoryId = verificationCase?.FactoryId ?? 0;

            return BaseResponse<AiVerificationResultDto>.Success(ToDto(result, verificationCaseId, factoryId));
        }

        private async Task<VerificationCase> GetOrCreateOpenCaseAsync(int factoryId)
        {
            var cases = await _uow.Repository<VerificationCase>()
                .FindAsync(c => c.FactoryId == factoryId);

            var latestOpen = cases
                .Where(c => c.Status != VerificationCaseStatus.Decided)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            return latestOpen ?? new VerificationCase
            {
                FactoryId = factoryId,
                Status = VerificationCaseStatus.UnderReview,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static string BuildSystemPrompt() =>
@"You are a KYB (Know-Your-Business) verification officer for an Egyptian industrial marketplace.
You are given a company's self-declared data and images/PDFs of its official documents
(commercial registry, tax card, ...). Read the documents, extract the key fields, and compare
them to the declared data.

Return EXCLUSIVELY a single raw JSON object (no markdown, no prose) with this exact shape:
{
  ""extractedFields"": { ""legalName"": null, ""commercialRegistryNo"": null, ""taxCardNo"": null, ""address"": null },
  ""confidenceScore"": 0.0,
  ""mismatches"": [ { ""field"": "".."", ""declared"": "".."", ""found"": "".."" } ],
  ""recommendation"": ""approve""
}

Rules:
- confidenceScore is a number between 0 and 1.
- recommendation must be one of: approve, review, reject.
- If a field cannot be read, set it to null. If there are no mismatches, return an empty array.
- Recommend 'reject' for clear mismatches, 'review' for low confidence or missing documents, 'approve' otherwise.";

        private static string BuildUserPrompt(Factory factory, IReadOnlyList<Document> documents)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Company self-declared data:");
            sb.AppendLine($"- LegalName: {factory.LegalName}");
            sb.AppendLine($"- CommercialRegistryNo: {factory.CommercialRegistryNo}");
            sb.AppendLine($"- TaxCardNo: {factory.TaxCardNo}");
            sb.AppendLine($"- Sector: {factory.Sector}");
            sb.AppendLine($"- Address: {factory.Address}");
            sb.AppendLine();
            sb.AppendLine($"Attached documents ({documents.Count} total, up to {MaxDocumentsAnalyzed} analyzed):");
            foreach (var doc in documents.Take(MaxDocumentsAnalyzed))
                sb.AppendLine($"- {doc.DocumentType}: {doc.FileUrl}");
            return sb.ToString();
        }

        private static AnalysisResult ParseAnalysis(string raw)
        {
            var result = new AnalysisResult
            {
                ExtractedFields = "{}",
                ConfidenceScore = 0m,
                Mismatches = null,
                Recommendation = AIRecommendation.Review
            };

            try
            {
                using var doc = JsonDocument.Parse(AiJson.Clean(raw));
                var root = doc.RootElement;

                if (root.TryGetProperty("extractedFields", out var fields))
                    result.ExtractedFields = fields.GetRawText();

                if (root.TryGetProperty("confidenceScore", out var score) &&
                    score.ValueKind == JsonValueKind.Number)
                    result.ConfidenceScore = Math.Clamp(score.GetDecimal(), 0m, 1m);

                if (root.TryGetProperty("mismatches", out var mismatches) &&
                    mismatches.ValueKind != JsonValueKind.Null)
                {
                    var text = mismatches.GetRawText();
                    result.Mismatches = text == "[]" ? null : text;
                }

                if (root.TryGetProperty("recommendation", out var rec) &&
                    rec.ValueKind == JsonValueKind.String &&
                    Enum.TryParse<AIRecommendation>(rec.GetString(), ignoreCase: true, out var parsed))
                    result.Recommendation = parsed;
            }
            catch
            {
                // Keep the safe defaults (Review, 0 confidence) on any parse failure.
            }

            return result;
        }

        private static AiVerificationResultDto ToDto(AIVerificationResult e, int caseId, int factoryId) => new()
        {
            Id = e.Id,
            VerificationCaseId = caseId,
            FactoryId = factoryId,
            ExtractedFields = e.ExtractedFields,
            ConfidenceScore = e.ConfidenceScore,
            Mismatches = e.Mismatches,
            Recommendation = e.AIRecommendation.ToString().ToLowerInvariant(),
            ModelVersion = e.ModelVersion,
            CreatedAt = e.CreatedAt
        };

        private sealed class AnalysisResult
        {
            public string ExtractedFields { get; set; } = "{}";
            public decimal ConfidenceScore { get; set; }
            public string? Mismatches { get; set; }
            public AIRecommendation Recommendation { get; set; }
        }
    }
}

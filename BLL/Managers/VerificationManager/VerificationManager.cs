using System.Text;
using System.Text.Json;
using BLL.AI;
using BLL.AI.Abstractions;
using BLL.DTOs.Common;
using BLL.DTOs.Verification;
using BLL.Managers.CloudinaryManager;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;

namespace BLL.Managers.Verification
{
    public class VerificationManager : IVerificationManager
    {
        private const int MaxDocumentsAnalyzed = 5;

        private readonly IUnitOfWork _uow;
        private readonly IAiProviderResolver _providerResolver;
        private readonly IDocumentContentFetcher _documentFetcher;
        private readonly ICloudinaryService _cloudinary;

        public VerificationManager(
            IUnitOfWork uow,
            IAiProviderResolver providerResolver,
            IDocumentContentFetcher documentFetcher,
            ICloudinaryService cloudinary)
        {
            _uow = uow;
            _providerResolver = providerResolver;
            _documentFetcher = documentFetcher;
            _cloudinary = cloudinary;
        }

        // ── Verify-first: extract straight from the upload, store nothing ──────────────────────
        public async Task<BaseResponse<KybExtractionResultDto>> ExtractFromUploadAsync(
            int factoryId, int requestingUserId, IReadOnlyList<IFormFile> files,
            CancellationToken cancellationToken = default)
        {
            var factory = await _uow.Repository<Factory>().GetByIdAsync(factoryId);
            if (factory is null)
                return BaseResponse<KybExtractionResultDto>.Failure("Factory not found.", statusCode: 404);

            if (factory.UserId != requestingUserId)
                return BaseResponse<KybExtractionResultDto>.Failure(
                    "You can only submit KYB documents for your own factory.", statusCode: 403);

            if (files is null || files.Count == 0)
                return BaseResponse<KybExtractionResultDto>.Failure(
                    "At least one KYB document is required.", statusCode: 400);

            // 1. Size + extension gate (5 MB, .PNG/.JPG/.PDF) — reject before touching the AI gateway.
            foreach (var file in files)
            {
                var error = KybUpload.Validate(file);
                if (error is not null)
                    return BaseResponse<KybExtractionResultDto>.Failure(error, statusCode: 400);
            }

            // 2. Read the bytes and hand them to the active provider for extraction.
            var attachments = new List<AiAttachment>();
            foreach (var file in files.Take(MaxDocumentsAnalyzed))
                attachments.Add(await KybUpload.ToAttachmentAsync(file, cancellationToken));

            var descriptors = files.Take(MaxDocumentsAnalyzed)
                .Select(f => ("document", f.FileName))
                .ToList();

            var outcome = await AnalyzeAsync(factory, attachments, descriptors, cancellationToken);
            if (outcome.Error is not null)
                return BaseResponse<KybExtractionResultDto>.Failure(outcome.Error, statusCode: 502);

            var analysis = outcome.Analysis!;
            var dto = new KybExtractionResultDto
            {
                FactoryId = factoryId,
                ExtractedFields = analysis.ExtractedFields,
                ConfidenceScore = analysis.ConfidenceScore,
                Mismatches = analysis.Mismatches,
                Recommendation = analysis.Recommendation.ToString().ToLowerInvariant(),
                ModelVersion = outcome.ModelVersion,
                Files = files.Select(f => new AnalyzedFileDto
                {
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    SizeBytes = f.Length
                }).ToList()
            };

            return BaseResponse<KybExtractionResultDto>.Success(dto, "KYB documents analyzed.");
        }

        // ── Confirm: persist the (re-sent) documents + the approved extraction ─────────────────
        public async Task<BaseResponse<AiVerificationResultDto>> ConfirmAndStoreAsync(
            int factoryId, int requestingUserId, IReadOnlyList<IFormFile> files, KybConfirmRequest request,
            CancellationToken cancellationToken = default)
        {
            var factory = await _uow.Repository<Factory>().GetByIdAsync(factoryId);
            if (factory is null)
                return BaseResponse<AiVerificationResultDto>.Failure("Factory not found.", statusCode: 404);

            if (factory.UserId != requestingUserId)
                return BaseResponse<AiVerificationResultDto>.Failure(
                    "You can only submit KYB documents for your own factory.", statusCode: 403);

            if (files is null || files.Count == 0)
                return BaseResponse<AiVerificationResultDto>.Failure(
                    "The documents must be re-sent to be stored.", statusCode: 400);

            foreach (var file in files)
            {
                var error = KybUpload.Validate(file);
                if (error is not null)
                    return BaseResponse<AiVerificationResultDto>.Failure(error, statusCode: 400);
            }

            // 1. Upload each document to Cloudinary and record it.
            var docRepo = _uow.Repository<Document>();
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                string url;
                try
                {
                    url = await _cloudinary.UploadFileAsync(file, $"factories/{factoryId}/kyb");
                }
                catch (Exception ex)
                {
                    return BaseResponse<AiVerificationResultDto>.Failure(
                        $"File upload failed: {ex.Message}", statusCode: 502);
                }

                if (string.IsNullOrWhiteSpace(url))
                    return BaseResponse<AiVerificationResultDto>.Failure("File upload failed.", statusCode: 502);

                await docRepo.AddAsync(new Document
                {
                    FactoryId = factoryId,
                    DocumentType = DocumentTypeFor(request.DocumentTypes, i),
                    FileUrl = url,
                    UploadedAt = DateTime.UtcNow
                });
            }

            // 2. Persist the approved AI extraction onto a verification case (advisory; officer decides).
            var analysis = AnalysisResult.FromConfirmRequest(request);
            var (aiResult, caseId) = await PersistAnalysisAsync(
                factoryId, analysis, request.ModelVersion ?? "unspecified");

            return BaseResponse<AiVerificationResultDto>.Success(
                ToDto(aiResult, caseId, factoryId),
                "Documents stored and verification recorded.", statusCode: 201);
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

            var descriptors = documents.Take(MaxDocumentsAnalyzed)
                .Select(d => (d.DocumentType, d.FileUrl))
                .ToList();

            // 2. Ask the active AI provider to extract and cross-check the data.
            var outcome = await AnalyzeAsync(factory, attachments, descriptors);
            if (outcome.Error is not null)
                return BaseResponse<AiVerificationResultDto>.Failure(outcome.Error, statusCode: 502);

            // 3. Attach the result to a verification case.
            var (aiResult, caseId) = await PersistAnalysisAsync(factoryId, outcome.Analysis!, outcome.ModelVersion);

            return BaseResponse<AiVerificationResultDto>.Success(
                ToDto(aiResult, caseId, factoryId),
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

        public async Task<BaseResponse<VerificationCaseDto>> DecideCaseAsync(
            int verificationCaseId, int reviewerId, VerificationDecision decision, string? notes)
        {
            var caseRepo = _uow.Repository<VerificationCase>();
            var verificationCase = await caseRepo.GetByIdAsync(verificationCaseId);
            if (verificationCase is null)
                return BaseResponse<VerificationCaseDto>.Failure("Verification case not found.", statusCode: 404);

            var reviewer = await _uow.Repository<User>().GetByIdAsync(reviewerId);
            if (reviewer is null)
                return BaseResponse<VerificationCaseDto>.Failure("Reviewer (officer) not found.", statusCode: 400);

            var factory = await _uow.Repository<Factory>().GetByIdAsync(verificationCase.FactoryId);
            if (factory is null)
                return BaseResponse<VerificationCaseDto>.Failure("Factory not found.", statusCode: 404);

            // Close the case with the human decision.
            verificationCase.Status = VerificationCaseStatus.Decided;
            verificationCase.Decision = decision;
            verificationCase.ReviewerId = reviewerId;
            verificationCase.Notes = notes ?? verificationCase.Notes;
            verificationCase.DecidedAt = DateTime.UtcNow;
            caseRepo.Update(verificationCase);

            // Reflect the decision on the factory.
            factory.VerificationStatus = decision == VerificationDecision.Approved
                ? VerificationStatus.Verified
                : VerificationStatus.Rejected;
            _uow.Repository<Factory>().Update(factory);

            // Immutable audit trail of the admin action.
            var audit = new AuditLog
            {
                AdminId = reviewerId,
                Action = decision == VerificationDecision.Approved ? "approve_kyb" : "reject_kyb",
                TargetEntity = "Factory",
                TargetId = factory.Id,
                Payload = JsonSerializer.Serialize(new
                {
                    verificationCaseId,
                    decision = decision.ToString(),
                    notes,
                    factoryVerificationStatus = factory.VerificationStatus.ToString()
                }),
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Repository<AuditLog>().AddAsync(audit);

            await _uow.SaveChangesAsync();

            return BaseResponse<VerificationCaseDto>.Success(
                ToCaseDto(verificationCase, factory.VerificationStatus.ToString().ToLowerInvariant()),
                "Verification decision recorded.");
        }

        // ── Shared helpers ─────────────────────────────────────────────────────────────────────

        /// <summary>Runs the active provider over the attachments and parses the structured result.</summary>
        private async Task<ModelOutcome> AnalyzeAsync(
            Factory factory, IReadOnlyList<AiAttachment> attachments,
            IReadOnlyList<(string type, string label)> descriptors,
            CancellationToken cancellationToken = default)
        {
            var provider = _providerResolver.GetActiveProvider();
            try
            {
                var raw = await provider.CompleteAsync(new AiCompletionRequest
                {
                    SystemPrompt = BuildSystemPrompt(),
                    UserPrompt = BuildUserPrompt(factory, descriptors),
                    Attachments = attachments,
                    ExpectJson = true,
                    MaxOutputTokens = 1500
                }, cancellationToken);

                return new ModelOutcome { Analysis = ParseAnalysis(raw), ModelVersion = provider.ModelVersion };
            }
            catch (Exception ex)
            {
                return new ModelOutcome { Error = $"AI verification failed: {ex.Message}", ModelVersion = provider.ModelVersion };
            }
        }

        /// <summary>Saves an analysis result onto the factory's open case (delete-old/add-new on re-run).</summary>
        private async Task<(AIVerificationResult result, int caseId)> PersistAnalysisAsync(
            int factoryId, AnalysisResult analysis, string modelVersion)
        {
            var verificationCase = await GetOrCreateOpenCaseAsync(factoryId);

            var aiResult = new AIVerificationResult
            {
                ExtractedFields = analysis.ExtractedFields,
                ConfidenceScore = analysis.ConfidenceScore,
                Mismatches = analysis.Mismatches,
                AIRecommendation = analysis.Recommendation,
                ModelVersion = modelVersion,
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
            return (aiResult, verificationCase.Id);
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

        private static string DocumentTypeFor(List<string>? types, int index) =>
            types is not null && index < types.Count && !string.IsNullOrWhiteSpace(types[index])
                ? types[index]
                : "kyb";

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

        private static string BuildUserPrompt(Factory factory, IReadOnlyList<(string type, string label)> descriptors)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Company self-declared data:");
            sb.AppendLine($"- LegalName: {factory.LegalName}");
            sb.AppendLine($"- CommercialRegistryNo: {factory.CommercialRegistryNo}");
            sb.AppendLine($"- TaxCardNo: {factory.TaxCardNo}");
            sb.AppendLine($"- Sector: {factory.Sector}");
            sb.AppendLine($"- Address: {factory.Address}");
            sb.AppendLine();
            sb.AppendLine($"Attached documents ({descriptors.Count} total, up to {MaxDocumentsAnalyzed} analyzed):");
            foreach (var (type, label) in descriptors)
                sb.AppendLine($"- {type}: {label}");
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

        private static VerificationCaseDto ToCaseDto(VerificationCase c, string factoryVerificationStatus) => new()
        {
            Id = c.Id,
            FactoryId = c.FactoryId,
            Status = c.Status.ToString(),
            Decision = c.Decision?.ToString(),
            ReviewerId = c.ReviewerId,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt,
            DecidedAt = c.DecidedAt,
            FactoryVerificationStatus = factoryVerificationStatus
        };

        private sealed class ModelOutcome
        {
            public AnalysisResult? Analysis { get; set; }
            public string ModelVersion { get; set; } = "unspecified";
            public string? Error { get; set; }
        }

        private sealed class AnalysisResult
        {
            public string ExtractedFields { get; set; } = "{}";
            public decimal ConfidenceScore { get; set; }
            public string? Mismatches { get; set; }
            public AIRecommendation Recommendation { get; set; }

            /// <summary>Builds an analysis from a client-approved confirm payload (no AI call).</summary>
            public static AnalysisResult FromConfirmRequest(KybConfirmRequest request)
            {
                var recommendation = AIRecommendation.Review;
                if (!string.IsNullOrWhiteSpace(request.Recommendation))
                    Enum.TryParse(request.Recommendation, ignoreCase: true, out recommendation);

                return new AnalysisResult
                {
                    ExtractedFields = string.IsNullOrWhiteSpace(request.ExtractedFields) ? "{}" : request.ExtractedFields,
                    ConfidenceScore = Math.Clamp(request.ConfidenceScore, 0m, 1m),
                    Mismatches = string.IsNullOrWhiteSpace(request.Mismatches) ? null : request.Mismatches,
                    Recommendation = recommendation
                };
            }
        }
    }
}

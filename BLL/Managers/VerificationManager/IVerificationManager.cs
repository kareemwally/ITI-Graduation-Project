using BLL.DTOs.Common;
using BLL.DTOs.Verification;
using DAL.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace BLL.Managers.Verification
{
    /// <summary>
    /// AI-assisted KYB (Know-Your-Business) verification for factories. Reads the uploaded
    /// documents, extracts &amp; cross-checks the claimed data, and produces a recommendation.
    /// </summary>
    public interface IVerificationManager
    {
        /// <summary>
        /// "Verify-first" step: validates the uploaded documents (size + extension), sends their
        /// bytes straight to the AI gateway, and returns the extracted data for the user to review.
        /// Nothing is stored — neither the files nor the result.
        /// </summary>
        Task<BaseResponse<KybExtractionResultDto>> ExtractFromUploadAsync(
            int factoryId, int requestingUserId, IReadOnlyList<IFormFile> files,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// "Confirm" step: stores the (re-sent) documents to Cloudinary and persists the approved
        /// AI extraction result on a verification case, ready for an officer's decision.
        /// </summary>
        Task<BaseResponse<AiVerificationResultDto>> ConfirmAndStoreAsync(
            int factoryId, int requestingUserId, IReadOnlyList<IFormFile> files, KybConfirmRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>Runs an AI analysis over a factory's already-stored KYB documents and stores the result.</summary>
        Task<BaseResponse<AiVerificationResultDto>> RunAiVerificationAsync(int factoryId);

        /// <summary>Returns the AI result stored for a given verification case, if any.</summary>
        Task<BaseResponse<AiVerificationResultDto>> GetResultByCaseAsync(int verificationCaseId);

        /// <summary>
        /// Records a human officer's final decision: closes the case, sets the factory's
        /// verification status, and writes an audit log entry.
        /// </summary>
        Task<BaseResponse<VerificationCaseDto>> DecideCaseAsync(
            int verificationCaseId, int reviewerId, VerificationDecision decision, string? notes);
    }
}

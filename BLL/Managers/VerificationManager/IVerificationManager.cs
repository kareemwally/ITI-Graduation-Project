using BLL.DTOs.Common;
using BLL.DTOs.Verification;

namespace BLL.Managers.Verification
{
    /// <summary>
    /// AI-assisted KYB (Know-Your-Business) verification for factories. Reads the uploaded
    /// documents, extracts &amp; cross-checks the claimed data, and produces a recommendation.
    /// </summary>
    public interface IVerificationManager
    {
        /// <summary>Runs an AI analysis over a factory's KYB documents and stores the result.</summary>
        Task<BaseResponse<AiVerificationResultDto>> RunAiVerificationAsync(int factoryId);

        /// <summary>Returns the AI result stored for a given verification case, if any.</summary>
        Task<BaseResponse<AiVerificationResultDto>> GetResultByCaseAsync(int verificationCaseId);
    }
}

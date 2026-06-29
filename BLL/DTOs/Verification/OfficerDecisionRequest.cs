using DAL.Models.Enums;

namespace BLL.DTOs.Verification
{
    /// <summary>A human verification officer's final decision on a KYB case.</summary>
    public class OfficerDecisionRequest
    {
        public VerificationDecision Decision { get; set; }
        public string? Notes { get; set; }

        /// <summary>
        /// The deciding officer's user id. Used as a fallback when the request is not authenticated
        /// (otherwise the id is taken from the caller's claims).
        /// </summary>
        public int? ReviewerId { get; set; }
    }
}

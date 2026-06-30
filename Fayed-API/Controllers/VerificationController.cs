using BLL.DTOs.Common;
using BLL.DTOs.Verification;
using BLL.Managers.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // authenticated by default; each action narrows to its role below
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationManager _verificationManager;

        public VerificationController(IVerificationManager verificationManager)
        {
            _verificationManager = verificationManager;
        }

        // ── Factory-owner: verify-first KYB submission ─────────────────────────────────────────

        // Step 1: upload the KYB documents for analysis. The controller/manager validates size
        // (<=5MB) and extension (.PNG/.JPG/.PDF), then sends the bytes to the AI gateway and
        // returns the extracted data. Nothing is stored at this stage.
        [HttpPost("factories/{factoryId:int}/extract")]
        [Authorize(Roles = "Factory")]
        [RequestSizeLimit(30_000_000)] // a few 5 MB docs in one request
        public async Task<IActionResult> ExtractFromUpload(
            int factoryId, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(BaseResponse.Failure("Invalid token.", statusCode: 401));

            var response = await _verificationManager.ExtractFromUploadAsync(
                factoryId, userId, files, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        // Step 2: once the user confirms the extracted data on the frontend, re-send the same
        // documents together with the approved extraction. The documents are stored (Cloudinary)
        // and the result is persisted on a verification case for an officer to decide.
        [HttpPost("factories/{factoryId:int}/confirm")]
        [Authorize(Roles = "Factory")]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> ConfirmAndStore(
            int factoryId, [FromForm] List<IFormFile> files, [FromForm] KybConfirmRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(BaseResponse.Failure("Invalid token.", statusCode: 401));

            var response = await _verificationManager.ConfirmAndStoreAsync(
                factoryId, userId, files, request, cancellationToken);
            return StatusCode(response.StatusCode, response);
        }

        // ── Back-office (admin) ────────────────────────────────────────────────────────────────

        // Run an AI KYB analysis over a factory's already-stored documents and store the result.
        [HttpPost("factories/{factoryId:int}/ai-run")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RunAiVerification(int factoryId)
        {
            var response = await _verificationManager.RunAiVerificationAsync(factoryId);
            return StatusCode(response.StatusCode, response);
        }

        // Fetch the AI result attached to a specific verification case.
        [HttpGet("cases/{caseId:int}/ai-result")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAiResult(int caseId)
        {
            var response = await _verificationManager.GetResultByCaseAsync(caseId);
            return StatusCode(response.StatusCode, response);
        }

        // Human officer's final decision: closes the case, sets the factory's verification status,
        // and writes an audit log. The AI recommendation is only advisory — this is the binding step.
        [HttpPost("cases/{caseId:int}/decision")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DecideCase(int caseId, [FromBody] OfficerDecisionRequest request)
        {
            // Prefer the authenticated officer's id; fall back to the body when not authenticated.
            var reviewerId = TryGetUserId(out var uid) ? uid : request.ReviewerId ?? 0;

            if (reviewerId <= 0)
                return BadRequest(BaseResponse.Failure("A reviewer (officer) id is required.", statusCode: 400));

            var response = await _verificationManager.DecideCaseAsync(
                caseId, reviewerId, request.Decision, request.Notes);
            return StatusCode(response.StatusCode, response);
        }

        private bool TryGetUserId(out int userId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId) && userId > 0;
    }
}

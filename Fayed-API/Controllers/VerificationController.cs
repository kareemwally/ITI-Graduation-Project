using BLL.DTOs.Common;
using BLL.DTOs.Verification;
using BLL.Managers.Verification;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationManager _verificationManager;

        public VerificationController(IVerificationManager verificationManager)
        {
            _verificationManager = verificationManager;
        }

        // Run an AI KYB analysis over a factory's uploaded documents and store the result.
        [HttpPost("factories/{factoryId:int}/ai-run")]
        public async Task<IActionResult> RunAiVerification(int factoryId)
        {
            var response = await _verificationManager.RunAiVerificationAsync(factoryId);
            return StatusCode(response.StatusCode, response);
        }

        // Fetch the AI result attached to a specific verification case.
        [HttpGet("cases/{caseId:int}/ai-result")]
        public async Task<IActionResult> GetAiResult(int caseId)
        {
            var response = await _verificationManager.GetResultByCaseAsync(caseId);
            return StatusCode(response.StatusCode, response);
        }

        // Human officer's final decision: closes the case, sets the factory's verification status,
        // and writes an audit log. The AI recommendation is only advisory — this is the binding step.
        [HttpPost("cases/{caseId:int}/decision")]
        public async Task<IActionResult> DecideCase(int caseId, [FromBody] OfficerDecisionRequest request)
        {
            // Prefer the authenticated officer's id; fall back to the body when not authenticated.
            var reviewerId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)
                ? uid
                : request.ReviewerId ?? 0;

            if (reviewerId <= 0)
                return BadRequest(BaseResponse.Failure("A reviewer (officer) id is required.", statusCode: 400));

            var response = await _verificationManager.DecideCaseAsync(
                caseId, reviewerId, request.Decision, request.Notes);
            return StatusCode(response.StatusCode, response);
        }
    }
}

using BLL.Managers.Verification;
using Microsoft.AspNetCore.Mvc;

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
    }
}

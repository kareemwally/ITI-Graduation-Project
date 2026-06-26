using BLL.DTOs.AuthnticationDTOs;
using BLL.DTOs.Common;
using BLL.Managers.AuthnticationManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fayed_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-factory")]
        public async Task<IActionResult> RegisterFactory([FromForm] FayedRegisterFactoryRequest request)
        {
            var result = await _authService.RegisterFactoryAsync(request);
            return Ok(result);
        
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FayedLoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(result);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] FayedForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] FayedResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            return Ok(result);
        }

    }
}


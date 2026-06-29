
namespace BLL.DTOs.AuthnticationDTOs
{
    public class FayedResetPasswordRequest
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}

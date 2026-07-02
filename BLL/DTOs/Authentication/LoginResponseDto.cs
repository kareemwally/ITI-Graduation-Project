namespace BLL.DTOs.Authentication
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresOn { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}

using BLL.DTOs.AuthnticationDTOs;
using BLL.DTOs.Common; 
namespace BLL.Managers.AuthnticationManager
{
    public interface IAuthService
    {
        Task<BaseResponse> RegisterFactoryAsync(FayedRegisterFactoryRequest request);

        Task<BaseResponse> ConfirmEmailAsync(string userId, string token);

        Task<BaseResponse> ForgotPasswordAsync(string email);

        Task<BaseResponse> ResetPasswordAsync(FayedResetPasswordRequest request);

        Task<BaseResponse<LoginResponseDto>> LoginAsync(FayedLoginRequest request);
    }
}
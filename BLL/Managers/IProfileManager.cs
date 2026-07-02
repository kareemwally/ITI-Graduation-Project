using BLL.DTOs.Common;
using BLL.DTOs.Profile;

namespace BLL.Managers
{
    public interface IProfileManager
    {
        Task<BaseResponse<ProfileDto>> GetProfileAsync(int currentUserId);
        Task<BaseResponse<ProfileDto>> UpdateProfileAsync(int currentUserId, UpdateProfileDto dto);
    }
}

using BLL.DTOs.Common;
using BLL.DTOs.Disputes;

namespace BLL.Managers
{
    public interface IDisputeManager
    {
        Task<BaseResponse<List<DisputeDto>>> GetMyDisputesAsync(int currentUserId);
        Task<BaseResponse<DisputeDto>> GetDetailsAsync(int disputeId, int currentUserId);
        Task<BaseResponse<DisputeDto>> CreateAsync(int currentUserId, CreateDisputeDto dto);
        Task<BaseResponse<bool>> DeleteAsync(int disputeId);
    }
}

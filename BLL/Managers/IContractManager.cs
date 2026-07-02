using BLL.DTOs.Common;
using BLL.DTOs.Contracts;

namespace BLL.Managers
{
    public interface IContractManager
    {
        Task<BaseResponse<ContractFormDto>> GetFormAsync(int orderId, int currentUserId);
        Task<BaseResponse<ContractResponseDto>> SubmitContractAsync(int orderId, int currentUserId, SubmitContractDto dto);
        Task<BaseResponse<ContractResponseDto>> GetContractAsync(int orderId, int currentUserId);
        Task<BaseResponse<ContractResponseDto>> AcceptContractAsync(int orderId, int currentUserId);
        Task<BaseResponse<ContractResponseDto>> DeclineContractAsync(int orderId, int currentUserId, DeclineContractDto dto);
        Task<BaseResponse<PaymentSummaryDto>> GetPaymentSummaryAsync(int orderId, int currentUserId);
        Task<BaseResponse<PaymentResponseDto>> InitiatePaymentAsync(int orderId, int currentUserId);
        Task<BaseResponse<bool>> ConfirmPaymentCallbackAsync(int orderId, string paymentStatus);
    }
}

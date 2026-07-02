using BLL.DTOs.Common;
using BLL.DTOs.Orders;
using BLL.DTOs.UserDashboard;

namespace BLL.Managers
{
    public interface IOrderManager
    {
        Task<BaseResponse<OrderDetailsDto>> CreateAsync(CreateOrderDto dto);
        Task<BaseResponse<OrderDetailsDto>> GetByIdAsync(int id);

        Task<BaseResponse<List<ConfirmedOrderDashboardDto>>> GetMyPurchasesAsync(int currentUserId);
        Task<BaseResponse<List<ConfirmedOrderDashboardDto>>> GetMySalesAsync(int currentUserId);

        Task<BaseResponse<ActiveOrderDetailDto>> GetActiveOrderDetailAsync(int orderId, int currentUserId);
        Task<BaseResponse<bool>> CancelOrderAsync(int orderId, int currentUserId);
        Task<BaseResponse<bool>> CompleteOrderAsync(int orderId, int currentUserId);
    }
}

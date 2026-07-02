using BLL.DTOs.Common;
using BLL.DTOs.Orders;
using BLL.DTOs.UserDashboard;

namespace BLL.Managers
{
    public interface IOfferManager
    {
        Task<BaseResponse<List<OrderDashboardListDto>>> GetSentOffersAsync(int currentUserId);
        Task<BaseResponse<PagedResult<OrderDashboardListDto>>> GetReceivedOffersAsync(int currentUserId, int page = 1, int pageSize = 20);
        Task<BaseResponse<OfferDetailsPopUpDto>> GetOfferDetailsAsync(int orderId, int currentUserId);
        Task<BaseResponse<bool>> SubmitOfferFromMarketAsync(int buyerUserId, SubmitMarketOfferDto dto);
        Task<BaseResponse<bool>> UpdateOfferAsync(int orderId, int currentUserId, UpdateMarketOfferDto dto);
        Task<BaseResponse<bool>> AcceptOfferAsync(int orderId, int currentUserId);
        Task<BaseResponse<bool>> RejectOrCancelOfferAsync(int orderId, int currentUserId);
    }
}

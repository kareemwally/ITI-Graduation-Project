using BLL.DTOs.Common;
using BLL.DTOs.Offers;

namespace BLL.Managers
{
    public interface IPurchaseOfferManager
    {
        Task<BaseResponse<bool>> CreateOfferAsync(int buyerId, CreatePurchaseOfferDto dto);
        Task<BaseResponse<bool>> RespondToOfferAsync(int offerId, int sellerId, bool isAccepted);

        Task<BaseResponse<List<PurchaseOfferDto>>> GetBuyerOffersAsync(int buyerId);
        Task<BaseResponse<List<PurchaseOfferDto>>> GetSellerOffersAsync(int sellerId);
    }
}
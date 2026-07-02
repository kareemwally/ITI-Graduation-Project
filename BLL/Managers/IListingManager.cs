using BLL.DTOs.Common;
using BLL.DTOs.Listings;

namespace BLL.Managers
{
    /// <summary>Business operations for marketplace listings.</summary>
    public interface IListingManager
    {
        Task<BaseResponse<PagedResult<ListingDto>>> GetPublishedAsync(PublishedListingsFilterDto filter);
        Task<BaseResponse<ListingDetailsDto>> GetByIdAsync(int id);
        Task<BaseResponse<ListingDetailsDto>> CreateAsync(CreateListingDto dto);
        Task<BaseResponse<bool>> UpdateAsync(int id, UpdateListingDto dto);
        Task<BaseResponse<bool>> PublishAsync(int id);
        Task<BaseResponse<bool>> DeleteAsync(int id);

        Task<BaseResponse<List<ListingDto>>> GetByUserFactoryAsync(int userId);
    }
}
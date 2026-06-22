using BLL.DTOs.Common;
using BLL.DTOs.Listings;

namespace BLL.Managers
{
    /// <summary>Business operations for marketplace listings.</summary>
    public interface IListingManager
    {
        Task<PagedResult<ListingDto>> GetPublishedAsync(int page, int pageSize, string? materialType = null);
        Task<ListingDetailsDto?> GetByIdAsync(int id);
        Task<ListingDetailsDto> CreateAsync(CreateListingDto dto);
        Task<bool> UpdateAsync(int id, UpdateListingDto dto);
        Task<bool> PublishAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

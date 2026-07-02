using BLL.DTOs.Categories;
using BLL.DTOs.Common;

namespace BLL.Managers
{
    public interface ICategoryManager
    {
        Task<BaseResponse<List<CategoryDto>>> GetAllAsync();
        Task<BaseResponse<List<CategoryDto>>> GetTreeAsync();
        Task<BaseResponse<CategoryDto>> GetByIdAsync(int id);
        Task<BaseResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto);
        Task<BaseResponse<bool>> UpdateAsync(int id, UpdateCategoryDto dto);
        Task<BaseResponse<bool>> DeleteAsync(int id);
    }
}

using BLL.DTOs.Categories;

namespace BLL.Managers
{
    /// <summary>Business operations for the catalog category tree.</summary>
    public interface ICategoryManager
    {
        Task<IReadOnlyList<CategoryDto>> GetAllAsync();
        Task<IReadOnlyList<CategoryDto>> GetTreeAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
        Task<bool> UpdateAsync(int id, UpdateCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

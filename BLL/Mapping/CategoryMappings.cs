using BLL.DTOs.Categories;
using DAL.Models;

namespace BLL.Mapping
{
    /// <summary>Hand-written, allocation-light mapping between Category entities and DTOs.</summary>
    public static class CategoryMappings
    {
        public static CategoryDto ToDto(this Category entity) => new()
        {
            Id = entity.Id,
            ParentId = entity.ParentId,
            Name = entity.Name
        };

        public static Category ToEntity(this CreateCategoryDto dto) => new()
        {
            ParentId = dto.ParentId,
            Name = dto.Name
        };

        public static void Apply(this UpdateCategoryDto dto, Category entity)
        {
            entity.ParentId = dto.ParentId;
            entity.Name = dto.Name;
        }
    }
}

namespace BLL.DTOs.Categories
{
    public class UpdateCategoryDto
    {
        public int? ParentId { get; set; }
        public string Name { get; set; } = null!;
    }
}

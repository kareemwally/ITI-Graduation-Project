namespace BLL.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public int? ParentId { get; set; }
        public string Name { get; set; } = null!;
    }
}

namespace BLL.DTOs.Categories
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; } = null!;
        public List<CategoryDto> Children { get; set; } = new();
    }
}

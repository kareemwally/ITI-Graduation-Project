namespace BLL.DTOs.Common
{
    public class PublishedListingsFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public string? Location { get; set; }
        public int? CategoryId { get; set; }
        public string? CustomCategory { get; set; }
        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortDirection { get; set; }
    }
}

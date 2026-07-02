using DAL.Models.Enums;

namespace BLL.DTOs.Disputes
{
    public class DisputeDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = null!;
        public string ListingTitle { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DisputeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RaisedByName { get; set; } = null!;
    }
}

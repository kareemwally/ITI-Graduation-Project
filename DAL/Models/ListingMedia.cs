using DAL.Models.Common;
using DAL.Models.Enums;

namespace DAL.Models
{
    /// <summary>Image/video asset attached to a listing (suggested max 10 per listing).</summary>
    public class ListingMedia : BaseEntity
    {
        public int ListingId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public MediaType MediaType { get; set; }
        public bool IsMain { get; set; }

        // Navigation
        public Listing Listing { get; set; } = null!;
    }
}

namespace BLL.DTOs.UserDashboard
{
    public class OrderDashboardListDto
    {
        public int OrderId { get; set; }
        public string OfferCode { get; set; } = null!;     // OFF-404
        public string ClientCode { get; set; } = null!;    // FYD-2847
        public string ProductTitle { get; set; } = null!;  // خام HDPE
        public decimal TotalValue { get; set; }            // إجمالي القيمة ج.م
        public string Status { get; set; } = null!;        // قيد التفاوض، بانتظار الرد، مقبول...
        public DateTime SentAt { get; set; }
    }
}

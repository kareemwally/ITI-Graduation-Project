namespace BLL.DTOs.UserDashboard
{
    public class ConfirmedOrderDashboardDto
    {
        public int OrderId { get; set; }
        public string OrderCode => $"ORD-{OrderId + 600}"; // توليد كود ORD-632 زي السكرينة
        public string SellerCode { get; set; } = null!;    // كود المورد FYD-2847
        public string ProductTitle { get; set; } = null!;  // اسم المنتج
        public decimal TotalValue { get; set; }            // إجمالي القيمة ج.م
        public string Status { get; set; } = null!;        // قيد الشحن، قيد التجهيز، مكتمل
    }
}

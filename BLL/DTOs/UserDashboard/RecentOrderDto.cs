namespace BLL.DTOs.UserDashboard
{
    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; } = null!;
        public string Date { get; set; } = null!; // هنبعتها متفرمتة جاهزة (مثلا: 3 يونيو)
        public string QuantityWithUnit { get; set; } = null!; // (مثلا: 3 طن)
        public string Status { get; set; } = null!; // (قيد الانتظار، جار المعالجة...)
    }
}

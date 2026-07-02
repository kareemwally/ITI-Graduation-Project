using DAL.Models.Enums;

namespace BLL.Mapping.Orders
{
    public static class OrderStatusMapper
    {
        public static string ToArabicStatus(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "معلق",
                OrderStatus.InProgress => "قيد التشغيل",
                OrderStatus.Completed => "مكتمل",
                OrderStatus.Cancelled => "ملغي",
                OrderStatus.ContractReview => "قيد التشغيل",
                OrderStatus.PaymentPending => "قيد التشغيل",
                _ => "قيد التشغيل"
            };
        }
    }
}

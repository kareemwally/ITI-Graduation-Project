using BLL.DTOs.UserDashboard;
using BLL.Mapping.Orders;
using DAL.Models;
using DAL.Models.Enums;

namespace BLL.Mapping.Dashboard
{
    public static class DashboardMappingExtensions
    {
        public static RecentOrderDto ToRecentOrderDto(this Order order, int userId)
        {
            return new RecentOrderDto
            {
                OrderId = order.Id,
                ClientName = order.BuyerId == userId ? order.Seller.Name : order.Buyer.Name,
                Date = order.CreatedAt.ToString("dd MMMM"),
                QuantityWithUnit = $"{order.AgreedQuantity} {order.Listing.MeasureUnit}",
                Status = order.Status.ToArabicStatus()
            };
        }
    }
}

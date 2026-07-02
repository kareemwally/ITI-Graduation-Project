using BLL.DTOs.Orders;
using BLL.DTOs.UserDashboard;
using DAL.Models;
using DAL.Models.Enums;

namespace BLL.Mapping.Orders
{
    public static class OrderDashboardMappingExtensions
    {
        public static OrderDashboardListDto ToDashboardListDto(this Order order, PartyRole viewRole)
        {
            return new OrderDashboardListDto
            {
                OrderId = order.Id,
                OfferCode = $"OFF-{order.Id + 400}",
                ClientCode = viewRole == PartyRole.Buyer
                    ? $"FYD-{order.SellerId + 2000}"
                    : $"FYD-{order.BuyerId + 2000}",
                ProductTitle = order.Listing?.Title ?? "خامة غير معروفة",
                TotalValue = order.AgreedTotalPrice,
                SentAt = order.CreatedAt,
                Status = MapListStatus(order.Status, viewRole)
            };
        }

        public static OfferDetailsPopUpDto ToPopUpDto(this Order order, PartyRole viewRole)
        {
            return new OfferDetailsPopUpDto
            {
                OrderId = order.Id,
                OfferCode = $"OFF-{order.Id + 400}",
                ClientCode = viewRole == PartyRole.Buyer
                    ? $"FYD-{order.SellerId + 2000}"
                    : $"FYD-{order.BuyerId + 2000}",
                SentAt = order.CreatedAt,
                ProductTitle = order.Listing?.Title ?? "خامة غير معروفة",
                QuantityRequested = order.AgreedQuantity,
                PricePerUnit = order.AgreedTotalPrice / (order.AgreedQuantity == 0 ? 1 : order.AgreedQuantity),
                TotalValue = order.AgreedTotalPrice,
                Message = order.ProposedModification ?? "لا توجد رسائل إضافية.",
                Status = MapDetailStatus(order.Status, viewRole)
            };
        }

        public static ConfirmedOrderDashboardDto ToConfirmedOrderDto(this Order order)
        {
            return new ConfirmedOrderDashboardDto
            {
                OrderId = order.Id,
                SellerCode = $"FYD-{order.SellerId + 2000}",
                ProductTitle = order.Listing?.Title ?? "خامة مقبولة",
                TotalValue = order.AgreedTotalPrice,
                Status = order.Status.ToArabicStatus()
            };
        }

        private static string MapListStatus(OrderStatus status, PartyRole viewRole)
        {
            return status.ToArabicStatus();
        }

        private static string MapDetailStatus(OrderStatus status, PartyRole viewRole)
        {
            return status.ToArabicStatus();
        }
    }
}
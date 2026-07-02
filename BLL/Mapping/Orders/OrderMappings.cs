using BLL.DTOs.Orders;
using DAL.Models;
using DAL.Models.Enums;

namespace BLL.Mapping.Orders
{
    public static class OrderMappings
    {
        public static Order ToEntity(this CreateOrderDto dto)
        {
            return new Order
            {
                ListingId = dto.ListingId,
                BuyerId = dto.BuyerId,
                SellerId = dto.SellerId,
                AgreedQuantity = dto.Quantity
            };
        }

        public static OrderDto ToDto(this Order entity)
        {
            return new OrderDto
            {
                Id = entity.Id,
                ListingId = entity.ListingId,
                BuyerId = entity.BuyerId,
                SellerId = entity.SellerId,
                AgreedQuantity = entity.AgreedQuantity,
                TotalPrice = entity.AgreedTotalPrice,
                Status = entity.Status.ToArabicStatus(),
                CreatedAt = entity.CreatedAt
            };
        }

        public static OrderDetailsDto ToDetailsDto(this Order entity)
        {
            return new OrderDetailsDto
            {
                Id = entity.Id,
                ListingId = entity.ListingId,
                BuyerId = entity.BuyerId,
                SellerId = entity.SellerId,
                AgreedQuantity = entity.AgreedQuantity,
                TotalPrice = entity.AgreedTotalPrice,
                Status = entity.Status.ToArabicStatus(),
                CreatedAt = entity.CreatedAt,
                ProposedModification = entity.ProposedModification,
                PartyRole = entity.ProposedByRole?.ToString(),
                CommissionRate = entity.CommissionRate,
                PlatformCommission = entity.PlatformCommission,
                SellerTotalPayout = entity.SellerTotalPayout
            };
        }

        public static ActiveOrderDetailDto ToActiveOrderDetailDto(this Order entity, PartyRole viewRole)
        {
            return new ActiveOrderDetailDto
            {
                OrderId = entity.Id,
                OfferCode = $"OFF-{entity.Id + 400}",
                ClientCode = viewRole == PartyRole.Buyer
                    ? $"FYD-{entity.SellerId + 2000}"
                    : $"FYD-{entity.BuyerId + 2000}",
                ListingName = entity.Listing?.Title ?? string.Empty,
                TotalQuantity = entity.AgreedQuantity,
                MeasureUnit = entity.Listing?.MeasureUnit ?? string.Empty,
                TotalPrice = entity.AgreedTotalPrice,
                Status = entity.Status.ToArabicStatus(),
                DeliveryType = entity.DeliveryType,
                DeliveryDate = entity.DeliveryDate,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}

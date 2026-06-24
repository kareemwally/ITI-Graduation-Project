using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.Orders;
using DAL.Models;
namespace BLL.Mapping
{
    public static class OrderMappings
    {
        // الدالة دي بتحول من DTO لـ Entity وقت إنشاء طلب جديد
        public static Order ToEntity(this CreateOrderDto dto)
        {
            return new Order
            {
                ListingId = dto.ListingId,
               BuyerId  = dto.BuyerFactoryId,
                // لو جدول الـ Order فيه حقل للكمية أو البائع ضيفهم هنا
            };
        }

        // الدالة دي بتحول من Entity لـ DTO عشان نعرض البيانات للفرونت إند
        public static OrderDto ToDto(this Order entity)
        {
            return new OrderDto
            {
                Id = entity.Id,
                ListingId = entity.ListingId,
                BuyerFactoryId = entity.BuyerId,
                TotalPrice = entity.AgreedTotalPrice,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt
            };
        }

        // الدالة دي لعرض تفاصيل أكتر للطلب
        public static OrderDetailsDto ToDetailsDto(this Order entity)
        {
            return new OrderDetailsDto
            {
                Id = entity.Id,
                ListingId = entity.ListingId,
                BuyerFactoryId = entity.BuyerId,
                TotalPrice = entity.AgreedTotalPrice,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                Notes = null // لو عندك حقل للملاحظات في الداتا بيز غيرها
            };
        }
    }
}

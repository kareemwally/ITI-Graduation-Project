using DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public int BuyerFactoryId { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}

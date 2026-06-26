using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Orders
{
    public class CreateOrderDto
    {
        public int ListingId { get; set; }
        public int BuyerFactoryId { get; set; }
        public int SellerFactoryId { get; set; }
        public decimal Quantity { get; set; }
    }
}

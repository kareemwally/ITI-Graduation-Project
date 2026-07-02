using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Orders
{
        public class OrderDetailsDto : OrderDto
    {
        public string? Notes { get; set; }
        public string? ProposedModification { get; set; }
        public string? PartyRole { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal SellerTotalPayout { get; set; }
    }
}

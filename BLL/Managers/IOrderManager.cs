using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.Orders;
using BLL.DTOs.Common;
using DAL.Models.Enums;
namespace BLL.Managers
{
    public interface IOrderManager
    {
        Task<OrderDetailsDto> CreateAsync(CreateOrderDto dto);
        Task<OrderDetailsDto?> GetByIdAsync(int id);
        Task<PagedResult<OrderDto>> GetOrdersByFactoryAsync(int factoryId, int page, int pageSize);
        Task<bool> UpdateStatusAsync(int id, OrderStatus newStatus);
    }
}
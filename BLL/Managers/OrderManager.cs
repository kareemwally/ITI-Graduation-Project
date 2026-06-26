using BLL.DTOs.Common;
using BLL.DTOs.Orders;
using BLL.Mapping;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Managers
{
    public class OrderManager : IOrderManager
    {
        private readonly IUnitOfWork _uow;

        public OrderManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<OrderDetailsDto> CreateAsync(CreateOrderDto dto)
        {
            var entity = dto.ToEntity();

            // بنحط القيم المبدئية لأي طلب جديد
            entity.Status = OrderStatus.PendingPayment;
            entity.CreatedAt = DateTime.UtcNow;

            await _uow.Repository<Order>().AddAsync(entity);
            await _uow.SaveChangesAsync();

            return entity.ToDetailsDto();
        }

        public async Task<OrderDetailsDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<Order>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            return entity?.ToDetailsDto();
        }

        public async Task<PagedResult<OrderDto>> GetOrdersByFactoryAsync(int factoryId, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            // استخدمنا BuyerId زي ما موجود عندك في الـ Entity بالظبط
            var query = _uow.Repository<Order>().Query()
                .AsNoTracking()
                .Where(o => o.BuyerId == factoryId);

            var total = await query.CountAsync();

            var entities = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<OrderDto>
            {
                Items = entities.Select(o => o.ToDto()).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<bool> UpdateStatusAsync(int id, OrderStatus newStatus)
        {
            var repo = _uow.Repository<Order>();
            var entity = await repo.GetByIdAsync(id);

            if (entity is null)
                return false;

            entity.Status = newStatus;

            repo.Update(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
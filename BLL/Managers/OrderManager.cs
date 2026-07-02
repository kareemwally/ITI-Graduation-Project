using BLL.DTOs.Common;
using BLL.DTOs.Orders;
using BLL.DTOs.UserDashboard;
using BLL.Mapping.Orders;
using DAL.Models;
using DAL.Models.Enums;
using DAL.Models.ExceptionModels;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class OrderManager : IOrderManager
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public OrderManager(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<OrderDetailsDto>> CreateAsync(CreateOrderDto dto)
        {
            var entity = dto.ToEntity();
            entity.Status = OrderStatus.Pending;
            entity.CreatedAt = DateTime.UtcNow;

            await _uow.Repository<Order>().AddAsync(entity);
            await _uow.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                entity.SellerId,
                "طلب شراء جديد",
                $"تم إنشاء طلب شراء جديد برقم {entity.Id}.",
                "order_created",
                $"/orders/{entity.Id}");

            return BaseResponse<OrderDetailsDto>.Success(entity.ToDetailsDto(), "تم إنشاء الطلب بنجاح.");
        }

        public async Task<BaseResponse<OrderDetailsDto>> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<Order>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (entity == null)
                throw new OrderNotFoundException(id);

            return BaseResponse<OrderDetailsDto>.Success(entity.ToDetailsDto(), "تم جلب الطلب بنجاح.");
        }

        public async Task<BaseResponse<List<ConfirmedOrderDashboardDto>>> GetMyPurchasesAsync(int currentUserId)
        {
            var orders = await _uow.Repository<Order>().Query()
                .AsNoTracking()
                .Include(o => o.Listing)
                .Where(o => o.BuyerId == currentUserId &&
                            o.Status == OrderStatus.InProgress)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var dtos = orders.Select(o => o.ToConfirmedOrderDto()).ToList();
            return BaseResponse<List<ConfirmedOrderDashboardDto>>.Success(dtos, "تم جلب المشتريات المؤكدة.");
        }

        public async Task<BaseResponse<List<ConfirmedOrderDashboardDto>>> GetMySalesAsync(int currentUserId)
        {
            var orders = await _uow.Repository<Order>().Query()
                .AsNoTracking()
                .Include(o => o.Listing)
                .Where(o => o.SellerId == currentUserId &&
                            o.Status == OrderStatus.InProgress)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var dtos = orders.Select(o => o.ToConfirmedOrderDto()).ToList();
            return BaseResponse<List<ConfirmedOrderDashboardDto>>.Success(dtos, "تم جلب المبيعات المؤكدة.");
        }

        public async Task<BaseResponse<ActiveOrderDetailDto>> GetActiveOrderDetailAsync(int orderId, int currentUserId)
        {
            var order = await _uow.Repository<Order>().Query()
                .AsNoTracking()
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.BuyerId != currentUserId && order.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            if (order.Status != OrderStatus.InProgress)
                throw new InvalidOrderStateException("هذا الطلب ليس قيد التشغيل.");

            var role = order.BuyerId == currentUserId ? PartyRole.Buyer : PartyRole.Seller;
            return BaseResponse<ActiveOrderDetailDto>.Success(order.ToActiveOrderDetailDto(role), "تم جلب تفاصيل الطلب.");
        }

        public async Task<BaseResponse<bool>> CompleteOrderAsync(int orderId, int currentUserId)
        {
            var repo = _uow.Repository<Order>();
            var order = await repo.Query()
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            if (order.Status != OrderStatus.InProgress)
                throw new InvalidOrderStateException("يمكن إكمال الطلبات قيد التشغيل فقط.");

            order.Status = OrderStatus.Completed;

            var listing = order.Listing;
            listing.Quantity -= order.AgreedQuantity;
            if (listing.Quantity <= 0)
            {
                listing.Quantity = 0;
                listing.Status = ListingStatus.Sold;
            }

            repo.Update(order);
            _uow.Repository<Listing>().Update(listing);
            await _uow.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                order.BuyerId,
                "تم اكتمال الطلب",
                $"تم اكتمال الطلب رقم {order.Id} بنجاح.",
                "order_completed",
                $"/orders/{order.Id}");

            return BaseResponse<bool>.Success(true, "تم إكمال الطلب وتحديث المخزون.");
        }

        public async Task<BaseResponse<bool>> CancelOrderAsync(int orderId, int currentUserId)
        {
            var repo = _uow.Repository<Order>();
            var order = await repo.GetByIdAsync(orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.BuyerId != currentUserId && order.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            if (order.Status != OrderStatus.InProgress)
                throw new InvalidOrderStateException("يمكن إلغاء الطلبات قيد التشغيل فقط.");

            order.Status = OrderStatus.Cancelled;
            repo.Update(order);
            await _uow.SaveChangesAsync();

            var otherPartyId = order.BuyerId == currentUserId ? order.SellerId : order.BuyerId;
            await _notificationService.SendNotificationAsync(
                otherPartyId,
                "تم إلغاء الطلب",
                $"تم إلغاء الطلب رقم {order.Id}.",
                "order_cancelled");

            return BaseResponse<bool>.Success(true, "تم إلغاء الطلب.");
        }
    }
}

using BLL.DTOs.Common;
using BLL.DTOs.Disputes;
using DAL.Models;
using DAL.Models.Enums;
using DAL.Models.ExceptionModels;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class DisputeManager : IDisputeManager
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public DisputeManager(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<List<DisputeDto>>> GetMyDisputesAsync(int currentUserId)
        {
            var disputes = await _uow.Repository<Dispute>().Query()
                .AsNoTracking()
                .Include(d => d.Order).ThenInclude(o => o.Listing)
                .Include(d => d.RaisedBy)
                .Where(d => d.Order.BuyerId == currentUserId || d.Order.SellerId == currentUserId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var dtos = disputes.Select(d => new DisputeDto
            {
                Id = d.Id,
                OrderId = d.OrderId,
                OrderCode = $"FYD-{d.OrderId + 2000}",
                ListingTitle = d.Order.Listing?.Title ?? string.Empty,
                Title = d.Title,
                Description = d.Description,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                RaisedByName = d.RaisedBy.Name
            }).ToList();

            return BaseResponse<List<DisputeDto>>.Success(dtos, "تم جلب النزاعات.");
        }

        public async Task<BaseResponse<DisputeDto>> GetDetailsAsync(int disputeId, int currentUserId)
        {
            var dispute = await _uow.Repository<Dispute>().Query()
                .AsNoTracking()
                .Include(d => d.Order).ThenInclude(o => o.Listing)
                .Include(d => d.RaisedBy)
                .FirstOrDefaultAsync(d => d.Id == disputeId);

            if (dispute == null)
                return BaseResponse<DisputeDto>.Failure("النزاع غير موجود.");

            if (dispute.Order.BuyerId != currentUserId && dispute.Order.SellerId != currentUserId)
                return BaseResponse<DisputeDto>.Failure("لا يمكنك الاطلاع على هذا النزاع.");

            var dto = new DisputeDto
            {
                Id = dispute.Id,
                OrderId = dispute.OrderId,
                OrderCode = $"FYD-{dispute.OrderId + 2000}",
                ListingTitle = dispute.Order.Listing?.Title ?? string.Empty,
                Title = dispute.Title,
                Description = dispute.Description,
                Status = dispute.Status,
                CreatedAt = dispute.CreatedAt,
                RaisedByName = dispute.RaisedBy.Name
            };

            return BaseResponse<DisputeDto>.Success(dto, "تم جلب تفاصيل النزاع.");
        }

        public async Task<BaseResponse<DisputeDto>> CreateAsync(int currentUserId, CreateDisputeDto dto)
        {
            var order = await _uow.Repository<Order>().Query()
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                return BaseResponse<DisputeDto>.Failure("الطلب غير موجود.");

            if (order.BuyerId != currentUserId && order.SellerId != currentUserId)
                return BaseResponse<DisputeDto>.Failure("لا يمكنك فتح نزاع على هذا الطلب.");

            if (order.IsDisputed)
                return BaseResponse<DisputeDto>.Failure("هناك نزاع مفتوح بالفعل على هذا الطلب.");

            var dispute = new Dispute
            {
                OrderId = dto.OrderId,
                RaisedById = currentUserId,
                Title = dto.Title,
                Description = dto.Description,
                Status = DisputeStatus.Opened,
                CreatedAt = DateTime.UtcNow
            };

            order.IsDisputed = true;

            await _uow.Repository<Dispute>().AddAsync(dispute);
            _uow.Repository<Order>().Update(order);
            await _uow.SaveChangesAsync();

            var otherPartyId = order.BuyerId == currentUserId ? order.SellerId : order.BuyerId;
            var listingTitle = order.Listing?.Title ?? string.Empty;

            await _notificationService.SendNotificationAsync(
                otherPartyId,
                "تم فتح نزاع",
                $"تم فتح نزاع على الطلب رقم {order.Id} على {listingTitle}. العنوان: {dto.Title}",
                "dispute_filed",
                $"/disputes/{dispute.Id}");

            await _notificationService.SendNotificationAsync(
                1, // Admin user ID — placeholder for admin notification
                "نزاع جديد",
                $"تم فتح نزاع جديد على الطلب رقم {order.Id} على {listingTitle}. العنوان: {dto.Title}",
                "dispute_filed",
                $"/disputes/{dispute.Id}");

            var resultDto = new DisputeDto
            {
                Id = dispute.Id,
                OrderId = dispute.OrderId,
                OrderCode = $"FYD-{dispute.OrderId + 2000}",
                ListingTitle = listingTitle,
                Title = dispute.Title,
                Description = dispute.Description,
                Status = dispute.Status,
                CreatedAt = dispute.CreatedAt,
                RaisedByName = (await _uow.Repository<User>().Query().AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId))?.Name ?? string.Empty
            };

            return BaseResponse<DisputeDto>.Success(resultDto, "تم فتح النزاع بنجاح.");
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int disputeId)
        {
            var dispute = await _uow.Repository<Dispute>().Query()
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.Id == disputeId);

            if (dispute == null)
                return BaseResponse<bool>.Failure("النزاع غير موجود.");

            // Restore order dispute status
            var hasOtherOpenDisputes = await _uow.Repository<Dispute>().Query()
                .AnyAsync(d => d.OrderId == dispute.OrderId && d.Id != disputeId && d.Status == DisputeStatus.Opened);

            if (!hasOtherOpenDisputes)
            {
                dispute.Order.IsDisputed = false;
                _uow.Repository<Order>().Update(dispute.Order);
            }

            dispute.IsDeleted = true;
            dispute.DeletedAt = DateTime.UtcNow;

            _uow.Repository<Dispute>().Update(dispute);
            await _uow.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "تم حذف النزاع.");
        }
    }
}

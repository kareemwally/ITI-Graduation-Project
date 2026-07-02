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
    public class OfferManager : IOfferManager
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public OfferManager(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<List<OrderDashboardListDto>>> GetSentOffersAsync(int currentUserId)
        {
            var orders = await _uow.Repository<Order>().Query()
                .AsNoTracking()
                .Include(o => o.Listing)
                .Where(o => o.BuyerId == currentUserId &&
                            o.Status != OrderStatus.InProgress)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var dtos = orders.Select(o => o.ToDashboardListDto(PartyRole.Buyer)).ToList();
            return BaseResponse<List<OrderDashboardListDto>>.Success(dtos, "تم جلب عروض التفاوض المرسلة.");
        }

        public async Task<BaseResponse<PagedResult<OrderDashboardListDto>>> GetReceivedOffersAsync(int currentUserId, int page = 1, int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _uow.Repository<Order>().Query()
                .AsNoTracking()
                .Include(o => o.Listing)
                .Where(o => o.SellerId == currentUserId &&
                            o.Status != OrderStatus.InProgress);

            var total = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = orders.Select(o => o.ToDashboardListDto(PartyRole.Seller)).ToList();

            var result = new PagedResult<OrderDashboardListDto>
            {
                Items = dtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return BaseResponse<PagedResult<OrderDashboardListDto>>.Success(result, "تم جلب عروض التفاوض المستلمة.");
        }

        public async Task<BaseResponse<OfferDetailsPopUpDto>> GetOfferDetailsAsync(int orderId, int currentUserId)
        {
            var order = await _uow.Repository<Order>().Query()
                .AsNoTracking()
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.BuyerId != currentUserId && order.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            var role = order.BuyerId == currentUserId ? PartyRole.Buyer : PartyRole.Seller;
            return BaseResponse<OfferDetailsPopUpDto>.Success(order.ToPopUpDto(role), "تم جلب تفاصيل العرض.");
        }

        public async Task<BaseResponse<bool>> SubmitOfferFromMarketAsync(int buyerUserId, SubmitMarketOfferDto dto)
        {
            var listing = await _uow.Repository<Listing>().Query()
                .Include(l => l.Factory)
                .FirstOrDefaultAsync(l => l.Id == dto.ListingId);

            if (listing == null)
                return BaseResponse<bool>.Failure("الخامة المطلوبة غير موجودة.");

            if (listing.Factory == null)
                return BaseResponse<bool>.Failure("المصنع صاحب الخامة غير موجود.");

            var newOffer = new Order
            {
                ListingId = dto.ListingId,
                BuyerId = buyerUserId,
                SellerId = listing.Factory.UserId,
                AgreedQuantity = dto.Quantity,
                AgreedTotalPrice = dto.Quantity * dto.PricePerUnit,
                Status = OrderStatus.Pending,
                ProposedModification = dto.BuyerNote,
                ProposedByRole = PartyRole.Buyer,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Repository<Order>().AddAsync(newOffer);
            await _uow.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                newOffer.SellerId,
                "عرض شراء جديد",
                $"تم استلام عرض شراء جديد على {listing.Title}.",
                "offer_received",
                $"/offers/{newOffer.Id}/details");

            return BaseResponse<bool>.Success(true, "تم إرسال عرض الشراء بنجاح.");
        }

        public async Task<BaseResponse<bool>> UpdateOfferAsync(int orderId, int currentUserId, UpdateMarketOfferDto dto)
        {
            var repo = _uow.Repository<Order>();
            var order = await repo.GetByIdAsync(orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.BuyerId != currentUserId)
                throw new OfferAccessDeniedException();

            if (order.Status != OrderStatus.Pending || order.ProposedByRole == null)
                throw new InvalidOrderStateException("لا يمكن تعديل العرض في حالته الحالية.");

            order.AgreedQuantity = dto.NewQuantity;
            order.AgreedTotalPrice = dto.NewQuantity * dto.NewPricePerUnit;
            order.ProposedModification = dto.NewBuyerNote;
            order.ProposedByRole = PartyRole.Buyer;

            repo.Update(order);
            await _uow.SaveChangesAsync();

            var listing = await _uow.Repository<Listing>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == order.ListingId);

            var listingTitle = listing?.Title ?? "الخامة";

            await _notificationService.SendNotificationAsync(
                order.SellerId,
                "تم تعديل العرض",
                $"تم تعديل عرض الشراء على {listingTitle}. يرجى مراجعة التعديلات.",
                "offer_updated",
                $"/offers/{order.Id}/details");

            return BaseResponse<bool>.Success(true, "تم تحديث العرض بنجاح.");
        }

        public async Task<BaseResponse<bool>> AcceptOfferAsync(int orderId, int currentUserId)
        {
            var repo = _uow.Repository<Order>();
            var order = await repo.GetByIdAsync(orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            order.ProposedModification = null;
            order.ProposedByRole = null;
            order.Status = OrderStatus.InProgress;

            repo.Update(order);

            // Auto-create chat between buyer and seller for this listing
            var existingChat = await _uow.Repository<Chat>().Query()
                .FirstOrDefaultAsync(c => c.ListingId == order.ListingId && c.BuyerId == order.BuyerId);

            if (existingChat == null)
            {
                var chat = new Chat
                {
                    ListingId = order.ListingId,
                    BuyerId = order.BuyerId,
                    SellerId = order.SellerId,
                    Status = ChatStatus.Open,
                    StartedAt = DateTime.UtcNow
                };
                await _uow.Repository<Chat>().AddAsync(chat);

                var systemMessage = new Message
                {
                    Chat = chat,
                    SenderId = currentUserId,
                    Content = "تم قبول عرضك. يمكنكم الآن متابعة الطلب والتواصل.",
                    MessageType = MessageType.System,
                    SentAt = DateTime.UtcNow
                };
                await _uow.Repository<Message>().AddAsync(systemMessage);
            }

            await _uow.SaveChangesAsync();

            var listing = await _uow.Repository<Listing>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == order.ListingId);

            var listingTitle = listing?.Title ?? "الخامة";

            await _notificationService.SendNotificationAsync(
                order.BuyerId,
                "تم قبول عرضك",
                $"تم قبول عرضك على {listingTitle}. يمكنك الآن متابعة الطلب والتواصل مع المورد.",
                "offer_accepted",
                $"/orders/{order.Id}");

            return BaseResponse<bool>.Success(true, "تم قبول العرض بنجاح، وتم فتح محادثة للتواصل.");
        }

        public async Task<BaseResponse<bool>> RejectOrCancelOfferAsync(int orderId, int currentUserId)
        {
            var repo = _uow.Repository<Order>();
            var order = await repo.GetByIdAsync(orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (order.BuyerId != currentUserId && order.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            order.Status = OrderStatus.Cancelled;
            repo.Update(order);
            await _uow.SaveChangesAsync();

            var otherPartyId = order.BuyerId == currentUserId ? order.SellerId : order.BuyerId;
            await _notificationService.SendNotificationAsync(
                otherPartyId,
                "تم رفض العرض",
                "للأسف، تم رفض العرض الخاص بك.",
                "offer_rejected");

            return BaseResponse<bool>.Success(true, "تم رفض/سحب العرض.");
        }
    }
}

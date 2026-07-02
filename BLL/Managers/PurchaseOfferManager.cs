using BLL.DTOs.Common;
using BLL.DTOs.Offers;
using DAL.Models;
using DAL.Models.Enums;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class PurchaseOfferManager : IPurchaseOfferManager
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseOfferManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<bool>> CreateOfferAsync(int buyerId, CreatePurchaseOfferDto dto)
        {
            var listing = await _unitOfWork.Repository<Listing>().GetByIdAsync(dto.ListingId);
            if (listing == null || listing.IsDeleted)
                return new BaseResponse<bool> { IsSuccess = false, Message = "عفواً، هذا الإعلان غير متاح." };

            if (dto.RequestedQuantity > listing.Quantity)
                return new BaseResponse<bool> { IsSuccess = false, Message = $"عفواً، الكمية المتاحة ({listing.Quantity} طن) أقل من المطلوبة." };

            var factory = await _unitOfWork.Repository<Factory>().GetByIdAsync(listing.FactoryId);
            if (factory == null) return new BaseResponse<bool> { IsSuccess = false, Message = "بيانات المصنع غير مكتملة." };
            if (factory.UserId == buyerId) return new BaseResponse<bool> { IsSuccess = false, Message = "لا يمكنك الشراء لنفسك." };

            var offer = new PurchaseOffer
            {
                ListingId = dto.ListingId,
                BuyerId = buyerId,
                RequestedQuantity = dto.RequestedQuantity,
                OfferedPricePerTon = dto.OfferedPricePerTon,
                TotalValue = dto.RequestedQuantity * dto.OfferedPricePerTon,
                BuyerMessage = dto.BuyerMessage,
                Status = OfferStatus.Pending
            };

            await _unitOfWork.Repository<PurchaseOffer>().AddAsync(offer);
            await _unitOfWork.Repository<Notification>().AddAsync(new Notification { UserId = factory.UserId, Message = "تلقيت عرض شراء جديد." });
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool> { IsSuccess = true, Data = true, Message = "تم إرسال العرض بنجاح." };
        }

        public async Task<BaseResponse<bool>> RespondToOfferAsync(int offerId, int sellerId, bool isAccepted)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var offer = await _unitOfWork.Repository<PurchaseOffer>().GetByIdAsync(offerId);
                if (offer == null) return new BaseResponse<bool> { IsSuccess = false, Message = "العرض غير موجود." };

                var listing = await _unitOfWork.Repository<Listing>().Query()
                    .Include(l => l.Factory)
                    .FirstOrDefaultAsync(l => l.Id == offer.ListingId);

                if (listing == null || listing.Factory == null)
                    return new BaseResponse<bool> { IsSuccess = false, Message = "بيانات الإعلان أو المصنع غير موجودة." };

                if (listing.Factory.UserId != sellerId)
                    return new BaseResponse<bool> { IsSuccess = false, Message = "غير مصرح لك بالرد." };

                if (offer.Status != OfferStatus.Pending)
                    return new BaseResponse<bool> { IsSuccess = false, Message = "تم الرد مسبقاً." };

                offer.Status = isAccepted ? OfferStatus.Accepted : OfferStatus.Rejected;

                if (isAccepted)
                {
                    if (listing.Quantity < offer.RequestedQuantity)
                        return new BaseResponse<bool> { IsSuccess = false, Message = "عفواً، الكمية المطلوبة لم تعد متاحة." };

                    listing.Quantity -= offer.RequestedQuantity;

                    var newOrder = new Order
                    {
                        ListingId = offer.ListingId,
                        BuyerId = offer.BuyerId,
                        SellerId = sellerId,
                        AgreedQuantity = offer.RequestedQuantity,
                        AgreedTotalPrice = offer.TotalValue,
                        Status = OrderStatus.PaymentPending,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Order>().AddAsync(newOrder);

                    var chat = await _unitOfWork.Repository<Chat>().Query()
                        .FirstOrDefaultAsync(c => c.ListingId == offer.ListingId && c.BuyerId == offer.BuyerId);

                    if (chat == null)
                    {
                        chat = new Chat { ListingId = offer.ListingId, BuyerId = offer.BuyerId, SellerId = sellerId };
                        // تم ربط الرسالة بالشات مباشرة قبل الحفظ لضمان الحفظ كعملية واحدة (Navigation Property)
                        chat.Messages = new List<Message>
                {
                    new Message { SenderId = sellerId, Content = "لقد قبلت طلبك بشكل مبدئي", SentAt = DateTime.UtcNow }
                };
                        await _unitOfWork.Repository<Chat>().AddAsync(chat);
                    }
                    else
                    {
                        await _unitOfWork.Repository<Message>().AddAsync(new Message
                        {
                            ChatId = chat.Id,
                            SenderId = sellerId,
                            Content = "لقد قبلت طلبك بشكل مبدئي",
                            SentAt = DateTime.UtcNow
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync(); // <-- حفظ مرة واحدة فقط!
                await transaction.CommitAsync();
                return new BaseResponse<bool> { IsSuccess = true, Data = true, Message = "تمت العملية بنجاح." };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BaseResponse<List<PurchaseOfferDto>>> GetBuyerOffersAsync(int buyerId)
        {
            var offers = await _unitOfWork.Repository<PurchaseOffer>().Query()
                .Include(o => o.Listing)
                .Where(o => o.BuyerId == buyerId && !o.IsDeleted)
                .Select(o => new PurchaseOfferDto { Id = o.Id, ListingId = o.ListingId, ListingTitle = o.Listing.Title, BuyerId = o.BuyerId, RequestedQuantity = o.RequestedQuantity, OfferedPricePerTon = o.OfferedPricePerTon, TotalValue = o.TotalValue, BuyerMessage = o.BuyerMessage, Status = o.Status.ToString() })
                .ToListAsync();
            return new BaseResponse<List<PurchaseOfferDto>> { IsSuccess = true, Data = offers };
        }

        public async Task<BaseResponse<List<PurchaseOfferDto>>> GetSellerOffersAsync(int sellerId)
        {
            var offers = await _unitOfWork.Repository<PurchaseOffer>().Query()
                .Include(o => o.Listing)
                .ThenInclude(l => l.Factory)
                .Where(o => o.Listing.Factory.UserId == sellerId && !o.IsDeleted)
                .Select(o => new PurchaseOfferDto { Id = o.Id, ListingId = o.ListingId, ListingTitle = o.Listing.Title, BuyerId = o.BuyerId, RequestedQuantity = o.RequestedQuantity, OfferedPricePerTon = o.OfferedPricePerTon, TotalValue = o.TotalValue, BuyerMessage = o.BuyerMessage, Status = o.Status.ToString() })
                .ToListAsync();
            return new BaseResponse<List<PurchaseOfferDto>> { IsSuccess = true, Data = offers };
        }
    }
}
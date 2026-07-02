using BLL.DTOs.Chat;
using BLL.DTOs.Common;
using BLL.Mapping.Chat;
using DAL.Models;
using DAL.Models.Enums;
using DAL.Models.ExceptionModels;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class ChatManager : IChatManager
    {
        private readonly IUnitOfWork _uow;

        public ChatManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<BaseResponse<List<ChatDto>>> GetMyChatsAsync(int currentUserId)
        {
            var chats = await _uow.Repository<Chat>().Query()
                .AsNoTracking()
                .Include(c => c.Listing)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                .Where(c => c.BuyerId == currentUserId || c.SellerId == currentUserId)
                .OrderByDescending(c => c.Messages.Any()
                    ? c.Messages.Max(m => m.SentAt)
                    : c.StartedAt)
                .ToListAsync();

            var dtos = chats.Select(c =>
            {
                var otherId = c.BuyerId == currentUserId ? c.SellerId : c.BuyerId;
                var otherCode = $"FYD-{otherId + 2000}";
                var lastMsg = c.Messages.MaxBy(m => m.SentAt)?.Content;
                var unread = c.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId);
                return c.ToChatDto(currentUserId, otherCode, lastMsg, unread);
            }).ToList();

            return BaseResponse<List<ChatDto>>.Success(dtos, "تم جلب المحادثات.");
        }

        public async Task<BaseResponse<ChatDetailsDto>> GetChatDetailsAsync(int chatId, int currentUserId)
        {
            var chat = await _uow.Repository<Chat>().Query()
                .AsNoTracking()
                .Include(c => c.Listing)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages.OrderBy(m => m.SentAt))
                .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new ChatNotFoundException(chatId);

            if (chat.BuyerId != currentUserId && chat.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            var dto = new ChatDetailsDto
            {
                Id = chat.Id,
                ListingId = chat.ListingId,
                ListingTitle = chat.Listing?.Title ?? string.Empty,
                BuyerId = chat.BuyerId,
                BuyerName = chat.Buyer.Name,
                SellerId = chat.SellerId,
                SellerName = chat.Seller.Name,
                Status = chat.Status,
                StartedAt = chat.StartedAt,
                Messages = chat.Messages.Select(m => m.ToDto(m.Sender.Name)).ToList()
            };

            return BaseResponse<ChatDetailsDto>.Success(dto, "تم جلب المحادثة.");
        }

        public async Task<BaseResponse<ChatDetailsDto>> CreateChatAsync(int currentUserId, CreateChatDto dto)
        {
            var listing = await _uow.Repository<Listing>().Query()
                .Include(l => l.Factory)
                .FirstOrDefaultAsync(l => l.Id == dto.ListingId);

            if (listing == null)
                return BaseResponse<ChatDetailsDto>.Failure("الخامة غير موجودة.");

            if (listing.Factory == null)
                return BaseResponse<ChatDetailsDto>.Failure("المصنع صاحب الخامة غير موجود.");

            var existingChat = await _uow.Repository<Chat>().Query()
                .FirstOrDefaultAsync(c => c.ListingId == dto.ListingId && c.BuyerId == currentUserId);

            if (existingChat != null)
                return BaseResponse<ChatDetailsDto>.Failure("محادثة مفتوحة بالفعل لهذه الخامة.");

            var chat = new Chat
            {
                ListingId = dto.ListingId,
                BuyerId = currentUserId,
                SellerId = listing.Factory.UserId,
                Status = ChatStatus.Open,
                StartedAt = DateTime.UtcNow
            };

            await _uow.Repository<Chat>().AddAsync(chat);
            await _uow.SaveChangesAsync();

            return await GetChatDetailsAsync(chat.Id, currentUserId);
        }

        public async Task<BaseResponse<MessageDto>> SendMessageAsync(int chatId, int currentUserId, SendMessageDto dto)
        {
            var chat = await _uow.Repository<Chat>().GetByIdAsync(chatId);

            if (chat == null)
                throw new ChatNotFoundException(chatId);

            if (chat.BuyerId != currentUserId && chat.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            if (chat.Status != ChatStatus.Open)
                return BaseResponse<MessageDto>.Failure("المحادثة مغلقة. لا يمكن إرسال رسائل جديدة.");

            var message = new Message
            {
                ChatId = chatId,
                SenderId = currentUserId,
                Content = dto.Content,
                MessageType = dto.MessageType,
                AttachmentUrl = dto.AttachmentUrl,
                SentAt = DateTime.UtcNow
            };

            await _uow.Repository<Message>().AddAsync(message);
            await _uow.SaveChangesAsync();

            var sender = await _uow.Repository<User>().GetByIdAsync(currentUserId);
            var dtoResult = message.ToDto(sender?.Name ?? "Unknown");

            return BaseResponse<MessageDto>.Success(dtoResult, "تم إرسال الرسالة.");
        }

        public async Task<BaseResponse<bool>> MarkMessagesAsReadAsync(int chatId, int currentUserId)
        {
            var chat = await _uow.Repository<Chat>().Query()
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new ChatNotFoundException(chatId);

            if (chat.BuyerId != currentUserId && chat.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            foreach (var msg in chat.Messages.Where(m => !m.IsRead && m.SenderId != currentUserId))
            {
                msg.IsRead = true;
            }

            await _uow.SaveChangesAsync();
            return BaseResponse<bool>.Success(true, "تم تحديد جميع الرسائل كمقروءة.");
        }

        public async Task<BaseResponse<bool>> UpdateChatStatusAsync(int chatId, int currentUserId, ChatStatus status)
        {
            var chat = await _uow.Repository<Chat>().GetByIdAsync(chatId);

            if (chat == null)
                throw new ChatNotFoundException(chatId);

            if (chat.BuyerId != currentUserId && chat.SellerId != currentUserId)
                throw new OfferAccessDeniedException();

            chat.Status = status;
            _uow.Repository<Chat>().Update(chat);
            await _uow.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "تم تحديث حالة المحادثة.");
        }
    }
}

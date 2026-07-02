using BLL.DTOs.Chat;
using BLL.DTOs.Common;
using DAL.Models.Enums;

namespace BLL.Managers
{
    public interface IChatManager
    {
        Task<BaseResponse<List<ChatDto>>> GetMyChatsAsync(int currentUserId);
        Task<BaseResponse<ChatDetailsDto>> GetChatDetailsAsync(int chatId, int currentUserId);
        Task<BaseResponse<ChatDetailsDto>> CreateChatAsync(int currentUserId, CreateChatDto dto);
        Task<BaseResponse<MessageDto>> SendMessageAsync(int chatId, int currentUserId, SendMessageDto dto);
        Task<BaseResponse<bool>> MarkMessagesAsReadAsync(int chatId, int currentUserId);
        Task<BaseResponse<bool>> UpdateChatStatusAsync(int chatId, int currentUserId, ChatStatus status);
    }
}

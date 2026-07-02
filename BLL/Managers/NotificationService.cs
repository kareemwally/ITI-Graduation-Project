using BLL.Managers.EmailService;
using DAL.Models;
using DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BLL.Managers
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;

        // Notification types that trigger email sending (high priority)
        private static readonly HashSet<string> _emailNotificationTypes = new()
        {
            "offer_received",
            "offer_accepted",
            "offer_rejected",
            "offer_updated",
            "order_created",
            "order_completed",
            "order_cancelled",
            "account_verified",
            "account_rejected",
            "down_payment_received",
            "contract_generated",
            "contract_accepted",
            "contract_declined",
            "payment_confirmed",
            "delivery_confirmed",
            "escrow_released",
            "commission_deducted",
            "dispute_filed",
            "dispute_resolved"
        };

        public NotificationService(IUnitOfWork uow, IEmailService emailService)
        {
            _uow = uow;
            _emailService = emailService;
        }

        public async Task SendNotificationAsync(int userId, string title, string message, string type, string? relatedLink = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedLink = relatedLink,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Repository<Notification>().AddAsync(notification);

            // Send email for important notifications
            if (_emailNotificationTypes.Contains(type))
            {
                try
                {
                    var user = await _uow.Repository<User>().Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user?.Email != null)
                    {
                        var emailBody = $@"
                            <div style='direction: rtl; font-family: Arial, sans-serif; text-align: right; padding: 20px;'>
                                <h2 style='color: #2b6cb0;'>{title}</h2>
                                <p>{message}</p>
                                {(relatedLink != null ? $"<div style='text-align: center; margin: 30px 0;'><a href='{relatedLink}' style='background-color: #3182ce; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>عرض التفاصيل</a></div>" : "")}
                                <hr style='border: 0; border-top: 1px solid #e2e8f0; margin-top: 40px;'>
                                <p style='font-size: 12px; color: #718096;'>منصة فايد - خطوتك نحو الاستقلالية والنمو.</p>
                            </div>";

                        await _emailService.SendEmailAsync(user.Email, title, emailBody);
                    }
                }
                catch
                {
                    // Email failure should not break the notification
                }
            }

            await _uow.SaveChangesAsync();
        }
    }
}

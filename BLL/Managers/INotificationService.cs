namespace BLL.Managers
{
    /// <summary>Internal service for creating notifications from other managers.</summary>
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string title, string message, string type, string? relatedLink = null);
    }
}

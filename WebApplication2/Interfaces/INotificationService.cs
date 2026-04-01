namespace WebApplication2.Interfaces;

public interface INotificationService
{
    Task<List<Models.Notification>> SendNotificationsAsync(Guid ticketId);
}
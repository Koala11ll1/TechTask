using WebApplication2.Interfaces;
using WebApplication2.Models;

namespace WebApplication2.Services;

public class NotificationService : INotificationService
{
    private readonly ITicketStorage _storage;
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationService(ITicketStorage storage, IEnumerable<INotificationSender> senders)
    {
        _storage = storage;
        _senders = senders;
    }

    public async Task<List<Notification>> SendNotificationsAsync(Guid ticketId)
    {
        var ticket = _storage.Tickets.FirstOrDefault(t => t.Id == ticketId);

        var notifications = _storage.Notifications
            .Where(notification => notification.TicketId == ticketId)
            .ToList();

        if (ticket == null) return notifications;

        foreach (var notification in notifications)
        {
            if (notification.Status != NotificationStatus.Sent && notification.Attempts < 3)
            {
                var sender = _senders.FirstOrDefault(s => s.Channel == notification.Channel);

                notification.Attempts++;

                try
                {
                    if (sender != null)
                    {
                        await sender.SendAsync(ticket.Title, ticket.Description);

                        notification.Status = NotificationStatus.Sent;
                        notification.LastError = null;
                    }
                    else
                    {
                        notification.LastError = $"Сендер для каналу {notification.Channel} не зареєстрований.";
                    }
                }
                catch (Exception ex)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.LastError = ex.Message;
                }
            }
        }

        return notifications;
    }
}
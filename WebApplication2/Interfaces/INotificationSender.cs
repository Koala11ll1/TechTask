using WebApplication2.Models;
using System.Threading.Tasks;

namespace WebApplication2.Interfaces;

public interface INotificationSender
{
    NotificationChannel Channel { get; }
    Task SendAsync(string title, string? description);
}


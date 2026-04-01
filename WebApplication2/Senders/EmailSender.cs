using WebApplication2.Models;
using WebApplication2.Interfaces;

namespace WebApplication2.Senders
{
    public class EmailSender : INotificationSender
    {
        public NotificationChannel Channel => NotificationChannel.Email;

        public async Task SendAsync(string title, string? description)
        {
            await Task.Delay(50);
            Console.WriteLine($"Email sent: {title}");
        }
    }
}

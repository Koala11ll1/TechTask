using System.Threading.Tasks;
using WebApplication2.Models;
using WebApplication2.Interfaces;

namespace WebApplication2.Senders
{
    public class SmsSender : INotificationSender
    {
        public NotificationChannel Channel => NotificationChannel.Sms;

        public async Task SendAsync(string title, string? description)
        {
            await Task.Delay(50);
            Console.WriteLine($"SMS sent: {title}");
        }
    }
}

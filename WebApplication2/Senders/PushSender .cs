using System;
using System.Threading.Tasks;
using WebApplication2.Models;
using WebApplication2.Interfaces;

namespace WebApplication2.Senders
{
    public class PushSender : INotificationSender
    {
        public NotificationChannel Channel => NotificationChannel.Push;

        public async Task SendAsync(string title, string? description)
        {
            await Task.Delay(50);
            Console.WriteLine($"Push sent: {title}");
        }
    }
}
using WebApplication2.Interfaces;
using WebApplication2.Senders;
using WebApplication2.Services;
using WebApplication2.Data;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<ITicketStorage, InMemoryTicketStorage>();
            builder.Services.AddControllers();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<INotificationSender, EmailSender>();
            builder.Services.AddScoped<INotificationSender, SmsSender>();
            builder.Services.AddScoped<INotificationSender, PushSender>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

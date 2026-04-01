namespace WebApplication2.Models
{
    public class Notification
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TicketId { get; set; }  
        public NotificationChannel Channel { get; set; }

        public NotificationStatus Status { get; set; }
        public int Attempts { get; set; }   
        public string? LastError { get; set; }
        public DateTimeOffset CreaterdAt { get; set; }

    }
}

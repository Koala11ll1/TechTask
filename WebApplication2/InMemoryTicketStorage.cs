using System.Collections.Generic;
using WebApplication2.Models;
using WebApplication2.Interfaces;


namespace WebApplication2.Data;

public class InMemoryTicketStorage : ITicketStorage
{
    public List<Ticket> Tickets { get; } = new List<Ticket>();
    public List<Notification> Notifications { get; } = new List<Notification>();
}
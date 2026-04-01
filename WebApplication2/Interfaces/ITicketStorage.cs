using System.Collections.Generic;
using WebApplication2.Models;


namespace WebApplication2.Interfaces;

public interface ITicketStorage
{
    List<Ticket> Tickets { get; }

    List<Notification> Notifications { get; }
}
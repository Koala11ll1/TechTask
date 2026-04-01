using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Interfaces;
using WebApplication2.Models;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketStorage _storage;
    private readonly INotificationService _notificationService;

    public TicketsController(ITicketStorage storage, INotificationService notificationService)
    {
        _storage = storage;
        _notificationService = notificationService;
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetTicket(Guid id)
    {
        var ticket = _storage.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new { message = $"Ticket with ID {id} not found." });
        }

        var notifications = _storage.Notifications
            .Where(n => n.TicketId == id)
            .ToList();

        return Ok(new
        {
            Ticket = ticket,
            Notifications = notifications
        });
    }

    [HttpPost]
    public IActionResult CreateTicket([FromBody] Ticket ticket)
    {
        _storage.Tickets.Add(ticket);

        var initialNotifications = new List<Notification>
        {
            new() { TicketId = ticket.Id, Channel = NotificationChannel.Email, Status = NotificationStatus.Pending },
            new() { TicketId = ticket.Id, Channel = NotificationChannel.Sms, Status = NotificationStatus.Pending },
            new() { TicketId = ticket.Id, Channel = NotificationChannel.Push, Status = NotificationStatus.Pending }
        };

        _storage.Notifications.AddRange(initialNotifications);

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
    }

    
    [HttpPost("{id:guid}/notify")]
    public async Task<IActionResult> Notify(Guid id)
    {
        
        var updatedNotifications = await _notificationService.SendNotificationsAsync(id);

        if (updatedNotifications == null || !updatedNotifications.Any())
        {
            return NotFound(new { message = "Сповіщень для цього тікета не знайдено." });
        }

        return Ok(updatedNotifications);
    }
} 
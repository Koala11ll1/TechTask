using Moq;
using System.Net.Sockets;
using WebApplication2.Interfaces;
using WebApplication2.Models;
using WebApplication2.Services;
using Xunit;

namespace WebApplication2.Tests;

public class NotificationServiceTests
{
    private readonly Mock<ITicketStorage> _storageMock;
    private readonly Mock<INotificationSender> _emailSenderMock;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _storageMock = new Mock<ITicketStorage>();
        _emailSenderMock = new Mock<INotificationSender>();
        _emailSenderMock.Setup(x => x.Channel).Returns(NotificationChannel.Email);

        _service = new NotificationService(_storageMock.Object, new[] { _emailSenderMock.Object });
    }

    // ТЕСТ 1: Перевірка ідемпотентності (якщо вже Sent, більше не відправляємо)
    [Fact]
    public async Task SendNotifications_ShouldNotSend_IfAlreadySent()
    {
        var ticketId = Guid.NewGuid();
        var notifications = new List<Notification> {
            new() { TicketId = ticketId, Channel = NotificationChannel.Email, Status = NotificationStatus.Sent, Attempts = 1 }
        };

        _storageMock.Setup(x => x.Tickets).Returns(new List<Ticket> { new() { Id = ticketId } });
        _storageMock.Setup(x => x.Notifications).Returns(notifications);

        await _service.SendNotificationsAsync(ticketId);

        _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
        Assert.Equal(1, notifications[0].Attempts);
    }

    // ТЕСТ 2: Перевірка ліміту спроб (не більше 3)
    [Fact]
    public async Task SendNotifications_ShouldNotExceedThreeAttempts()
    {
        var ticketId = Guid.NewGuid();
        var notifications = new List<Notification> {
            new() { TicketId = ticketId, Channel = NotificationChannel.Email, Status = NotificationStatus.Failed, Attempts = 3 }
        };

        _storageMock.Setup(x => x.Tickets).Returns(new List<Ticket> { new() { Id = ticketId } });
        _storageMock.Setup(x => x.Notifications).Returns(notifications);

        await _service.SendNotificationsAsync(ticketId);

        Assert.Equal(3, notifications[0].Attempts);
        _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    // ТЕСТ 3: Успішна відправка змінює статус на Sent
    [Fact]
    public async Task SendNotifications_ShouldSetStatusToSent_OnSuccess()
    {
        var ticketId = Guid.NewGuid();
        var notifications = new List<Notification> {
            new() { TicketId = ticketId, Channel = NotificationChannel.Email, Status = NotificationStatus.Pending }
        };

        _storageMock.Setup(x => x.Tickets).Returns(new List<Ticket> { new() { Id = ticketId, Title = "Test" } });
        _storageMock.Setup(x => x.Notifications).Returns(notifications);

        await _service.SendNotificationsAsync(ticketId);

        Assert.Equal(NotificationStatus.Sent, notifications[0].Status);
        Assert.Equal(1, notifications[0].Attempts);
    }

    // ТЕСТ 4: Помилка відправки змінює статус на Failed і записує LastError
    [Fact]
    public async Task SendNotifications_ShouldSetStatusToFailed_OnException()
    {
        var ticketId = Guid.NewGuid();
        var notifications = new List<Notification> {
            new() { TicketId = ticketId, Channel = NotificationChannel.Email, Status = NotificationStatus.Pending }
        };

        _emailSenderMock.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("SMTP Error"));

        _storageMock.Setup(x => x.Tickets).Returns(new List<Ticket> { new() { Id = ticketId, Title = "Test" } });
        _storageMock.Setup(x => x.Notifications).Returns(notifications);

        await _service.SendNotificationsAsync(ticketId);

        Assert.Equal(NotificationStatus.Failed, notifications[0].Status);
        Assert.Equal("SMTP Error", notifications[0].LastError);
    }

    // ТЕСТ 5: Перевірка, що ігноруються сповіщення інших тікетів
    [Fact]
    public async Task SendNotifications_ShouldOnlyProcessTargetTicket()
    {
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var notifications = new List<Notification> {
            new() { TicketId = otherId, Channel = NotificationChannel.Email, Status = NotificationStatus.Pending }
        };

        _storageMock.Setup(x => x.Tickets).Returns(new List<Ticket> { new() { Id = targetId } });
        _storageMock.Setup(x => x.Notifications).Returns(notifications);

        var result = await _service.SendNotificationsAsync(targetId);

        Assert.Empty(result.Where(n => n.TicketId == targetId));
        _emailSenderMock.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    // ТЕСТ 6: Перевірка, що спроби інкрементуються (1 -> 2)
    [Fact]
    public async Task SendNotifications_ShouldIncrementAttempts()
    {
        var ticketId = Guid.NewGuid();
        var notifications = new List<Notification> {
            new() { TicketId = ticketId, Channel = NotificationChannel.Email, Status = NotificationStatus.Failed, Attempts = 1 }
        };

        _storageMock.Setup(x => x.Tickets).Returns(new List<Ticket> { new() { Id = ticketId, Title = "Test" } });
        _storageMock.Setup(x => x.Notifications).Returns(notifications);

        await _service.SendNotificationsAsync(ticketId);

        Assert.Equal(2, notifications[0].Attempts);
    }
}
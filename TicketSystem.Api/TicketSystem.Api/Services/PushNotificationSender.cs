using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class PushNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Push;

    public Task SendAsync(Ticket ticket, Notification notification)
    {
        // Simulate channel-specific validation
        if (ticket.Description != null && string.IsNullOrWhiteSpace(ticket.Description))
        {
            throw new ArgumentException("Push sending failed: Description is whitespace.");
        }

        // Simulate successful delivery
        return Task.CompletedTask;
    }
}

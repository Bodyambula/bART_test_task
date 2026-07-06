using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class EmailNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public Task SendAsync(Ticket ticket, Notification notification)
    {
        // Simulate channel-specific validation
        if (string.IsNullOrWhiteSpace(ticket.Title))
        {
            throw new ArgumentException("Email sending failed: Ticket title is empty.");
        }

        // Simulate successful delivery
        return Task.CompletedTask;
    }
}

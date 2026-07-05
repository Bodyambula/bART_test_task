using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class EmailNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public Task SendAsync(Ticket ticket, Notification notification)
    {
        // Simulated validation rule for Email
        if (string.IsNullOrWhiteSpace(ticket.Title))
        {
            throw new ArgumentException("Email sending failed: Ticket title is empty.");
        }

        // In a real application, we would use an SMTP client or email API (like SendGrid).
        // For simulation, it succeeds.
        return Task.CompletedTask;
    }
}

using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class SmsNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    public Task SendAsync(Ticket ticket, Notification notification)
    {
        // Simulate channel-specific validation
        if (ticket.Title.Length > 160)
        {
            throw new ArgumentException("SMS sending failed: Content exceeds 160 characters.");
        }

        // Simulate successful delivery
        return Task.CompletedTask;
    }
}

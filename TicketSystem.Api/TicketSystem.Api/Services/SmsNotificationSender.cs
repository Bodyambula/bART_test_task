using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class SmsNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    public Task SendAsync(Ticket ticket, Notification notification)
    {
        // Simulated validation rule for SMS: SMS content (title) cannot exceed 160 characters
        if (ticket.Title.Length > 160)
        {
            throw new ArgumentException("SMS sending failed: Content exceeds 160 characters.");
        }

        // In a real application, we would use an SMS API (like Twilio).
        // For simulation, it succeeds.
        return Task.CompletedTask;
    }
}

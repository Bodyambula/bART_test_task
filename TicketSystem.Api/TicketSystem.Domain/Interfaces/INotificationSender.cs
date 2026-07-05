using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Models;

namespace TicketSystem.Domain.Interfaces;

public interface INotificationSender
{
    NotificationChannel Channel { get; }

    Task SendAsync(Ticket ticket, Notification notification);
}

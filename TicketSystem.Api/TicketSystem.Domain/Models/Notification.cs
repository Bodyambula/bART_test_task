using TicketSystem.Domain.Enums;

namespace TicketSystem.Domain.Models;

public class Notification
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public NotificationChannel Channel { get; set; }

    public NotificationStatus Status { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

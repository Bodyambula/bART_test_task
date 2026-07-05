using TicketSystem.Domain.Common;
using TicketSystem.Domain.Enums;

namespace TicketSystem.Domain.Models;

public class Notification
{
    public NotificationId Id { get; init; }

    public TicketId TicketId { get; init; }

    public NotificationChannel Channel { get; init; }

    public NotificationStatus Status { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset CreatedAt { get; init; }

    public Notification(NotificationId id, TicketId ticketId, NotificationChannel channel, NotificationStatus status, int attempts, string? lastError, DateTimeOffset createdAt)
    {
        this.Id = id;
        this.TicketId = ticketId;
        this.Channel = channel;
        this.Status = status;
        this.Attempts = attempts;
        this.LastError = lastError;
        this.CreatedAt = createdAt;
    }

    public bool CanAttempt => (this.Status == NotificationStatus.Pending || this.Status == NotificationStatus.Failed) && this.Attempts < 3;

    public void RecordAttempt()
    {
        if (!this.CanAttempt)
        {
            throw new InvalidOperationException("Notification cannot be retried.");
        }

        this.Attempts++;
    }

    public void MarkAsSent()
    {
        this.Status = NotificationStatus.Sent;
        this.LastError = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        this.Status = NotificationStatus.Failed;
        this.LastError = errorMessage;
    }
}

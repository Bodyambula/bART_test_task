using TicketSystem.Domain.Common;
using TicketSystem.Domain.Enums;

namespace TicketSystem.Domain.Models;

public class Ticket
{
    public TicketId Id { get; init; }

    public string Title { get; private set; }

    public string? Description { get; init; }

    public Priority Priority { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public Ticket(TicketId id, string title, string? description, Priority priority, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 5)
        {
            throw new ArgumentException("Title must be at least 5 characters long.", nameof(title));
        }

        this.Id = id;
        this.Title = title.Trim();
        this.Description = description;
        this.Priority = priority;
        this.CreatedAt = createdAt;
    }
}

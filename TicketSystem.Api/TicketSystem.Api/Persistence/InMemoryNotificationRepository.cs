using System.Collections.Concurrent;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly ConcurrentDictionary<Guid, Notification> notifications = new ();

    /// <inheritdoc/>
    public Task AddAsync(Notification notification)
    {
        this.notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Notification notification)
    {
        this.notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<List<Notification>> GetByTicketIdAsync(Guid ticketId)
    {
        var list = this.notifications.Values.Where(n => n.TicketId == ticketId).ToList();
        return Task.FromResult(list);
    }
}

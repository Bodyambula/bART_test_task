using System.Collections.Concurrent;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly ConcurrentDictionary<Guid, Notification> _notifications = new();

    public Task AddAsync(Notification notification)
    {
        _notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Notification notification)
    {
        _notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    public Task<List<Notification>> GetByTicketIdAsync(Guid ticketId)
    {
        var list = _notifications.Values.Where(n => n.TicketId == ticketId).ToList();
        return Task.FromResult(list);
    }
}

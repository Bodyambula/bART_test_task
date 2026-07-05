using System.Collections.Concurrent;
using TicketSystem.Domain.Common;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly ConcurrentDictionary<NotificationId, Notification> notifications = new ();

    public Task AddAsync(Notification notification)
    {
        this.notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Notification notification)
    {
        this.notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    public Task<List<Notification>> GetByTicketIdAsync(TicketId ticketId)
    {
        var list = this.notifications.Values.Where(n => n.TicketId == ticketId).ToList();
        return Task.FromResult(list);
    }
}

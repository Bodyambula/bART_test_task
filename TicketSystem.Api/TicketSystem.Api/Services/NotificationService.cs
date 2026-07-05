using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class NotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationService(
        INotificationRepository notificationRepository,
        ITicketRepository ticketRepository,
        IEnumerable<INotificationSender> senders)
    {
        _notificationRepository = notificationRepository;
        _ticketRepository = ticketRepository;
        _senders = senders;
    }

    public async Task CreatePendingNotificationsForTicketAsync(Guid ticketId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
        {
            throw new ArgumentException($"Ticket with ID {ticketId} does not exist.");
        }

        // Create a Pending notification for each channel
        var channels = Enum.GetValues<NotificationChannel>();
        foreach (var channel in channels)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                Channel = channel,
                Status = NotificationStatus.Pending,
                Attempts = 0,
                LastError = null,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _notificationRepository.AddAsync(notification);
        }
    }

    public async Task SendPendingOrFailedNotificationsAsync(Guid ticketId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
        {
            throw new ArgumentException($"Ticket with ID {ticketId} does not exist.");
        }

        var notifications = await _notificationRepository.GetByTicketIdAsync(ticketId);
        var sendersMap = _senders.ToDictionary(s => s.Channel);

        foreach (var notification in notifications)
        {
            // Only process Pending or Failed notifications with Attempts < 3
            if ((notification.Status == NotificationStatus.Pending || notification.Status == NotificationStatus.Failed) 
                && notification.Attempts < 3)
            {
                if (sendersMap.TryGetValue(notification.Channel, out var sender))
                {
                    notification.Attempts++;
                    try
                    {
                        await sender.SendAsync(ticket, notification);
                        notification.Status = NotificationStatus.Sent;
                        notification.LastError = null;
                    }
                    catch (Exception ex)
                    {
                        notification.Status = NotificationStatus.Failed;
                        notification.LastError = ex.Message;
                    }
                    await _notificationRepository.UpdateAsync(notification);
                }
            }
        }
    }
}

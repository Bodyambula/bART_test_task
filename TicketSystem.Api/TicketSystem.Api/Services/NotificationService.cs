using TicketSystem.Domain.Common;
using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class NotificationService
{
    private readonly INotificationRepository notificationRepository;
    private readonly ITicketRepository ticketRepository;
    private readonly IEnumerable<INotificationSender> senders;

    public NotificationService(
        INotificationRepository notificationRepository,
        ITicketRepository ticketRepository,
        IEnumerable<INotificationSender> senders)
    {
        this.notificationRepository = notificationRepository;
        this.ticketRepository = ticketRepository;
        this.senders = senders;
    }

    public async Task CreatePendingNotificationsForTicketAsync(TicketId ticketId)
    {
        var ticket = await this.ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
        {
            throw new ArgumentException($"Ticket with ID {ticketId} does not exist.");
        }

        // Create a Pending notification for each channel
        var channels = Enum.GetValues<NotificationChannel>();
        foreach (var channel in channels)
        {
            var notification = new Notification(
                NotificationId.New(),
                ticketId,
                channel,
                NotificationStatus.Pending,
                0,
                null,
                DateTimeOffset.UtcNow);
            await this.notificationRepository.AddAsync(notification);
        }
    }

    public async Task SendPendingOrFailedNotificationsAsync(TicketId ticketId)
    {
        var ticket = await this.ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
        {
            throw new ArgumentException($"Ticket with ID {ticketId} does not exist.");
        }

        var notifications = await this.notificationRepository.GetByTicketIdAsync(ticketId);
        var sendersMap = this.senders.ToDictionary(s => s.Channel);

        foreach (var notification in notifications)
        {
            if (notification.CanAttempt)
            {
                if (sendersMap.TryGetValue(notification.Channel, out var sender))
                {
                    notification.RecordAttempt();
                    try
                    {
                        await sender.SendAsync(ticket, notification);
                        notification.MarkAsSent();
                    }
                    catch (Exception ex)
                    {
                        notification.MarkAsFailed(ex.Message);
                    }

                    await this.notificationRepository.UpdateAsync(notification);
                }
            }
        }
    }
}

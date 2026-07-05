// <copyright file="NotificationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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

    public async Task CreatePendingNotificationsForTicketAsync(Guid ticketId)
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
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                Channel = channel,
                Status = NotificationStatus.Pending,
                Attempts = 0,
                LastError = null,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            await this.notificationRepository.AddAsync(notification);
        }
    }

    public async Task SendPendingOrFailedNotificationsAsync(Guid ticketId)
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

                    await this.notificationRepository.UpdateAsync(notification);
                }
            }
        }
    }
}

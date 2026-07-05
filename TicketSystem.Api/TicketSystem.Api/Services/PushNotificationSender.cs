// <copyright file="PushNotificationSender.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Services;

public class PushNotificationSender : INotificationSender
{
    /// <inheritdoc/>
    public NotificationChannel Channel => NotificationChannel.Push;

    /// <inheritdoc/>
    public Task SendAsync(Ticket ticket, Notification notification)
    {
        // Simulated validation rule for Push: Description if provided must not be empty or whitespace
        if (ticket.Description != null && string.IsNullOrWhiteSpace(ticket.Description))
        {
            throw new ArgumentException("Push sending failed: Description is whitespace.");
        }

        // In a real application, we would use Firebase Cloud Messaging (FCM) or Apple Push Notification service (APNs).
        // For simulation, it succeeds.
        return Task.CompletedTask;
    }
}

// <copyright file="INotificationSender.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Models;

namespace TicketSystem.Domain.Interfaces;

public interface INotificationSender
{
    NotificationChannel Channel { get; }

    Task SendAsync(Ticket ticket, Notification notification);
}

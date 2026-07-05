// <copyright file="INotificationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using TicketSystem.Domain.Models;

namespace TicketSystem.Domain.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);

    Task UpdateAsync(Notification notification);

    Task<List<Notification>> GetByTicketIdAsync(Guid ticketId);
}

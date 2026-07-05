// <copyright file="InMemoryTicketRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class InMemoryTicketRepository : ITicketRepository
{
    private readonly ConcurrentDictionary<Guid, Ticket> tickets = new ();

    /// <inheritdoc/>
    public Task AddAsync(Ticket ticket)
    {
        this.tickets[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<Ticket?> GetByIdAsync(Guid id)
    {
        this.tickets.TryGetValue(id, out var ticket);
        return Task.FromResult(ticket);
    }
}

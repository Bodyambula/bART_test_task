using System.Collections.Concurrent;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class InMemoryTicketRepository : ITicketRepository
{
    private readonly ConcurrentDictionary<Guid, Ticket> tickets = new ();

    public Task AddAsync(Ticket ticket)
    {
        this.tickets[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    public Task<Ticket?> GetByIdAsync(Guid id)
    {
        this.tickets.TryGetValue(id, out var ticket);
        return Task.FromResult(ticket);
    }
}

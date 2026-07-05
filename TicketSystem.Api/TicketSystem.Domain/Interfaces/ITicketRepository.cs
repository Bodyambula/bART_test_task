using TicketSystem.Domain.Common;
using TicketSystem.Domain.Models;

namespace TicketSystem.Domain.Interfaces;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket);

    Task<Ticket?> GetByIdAsync(TicketId id);
}

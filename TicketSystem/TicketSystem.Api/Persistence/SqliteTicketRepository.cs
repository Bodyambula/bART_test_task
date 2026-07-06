using Microsoft.EntityFrameworkCore;
using TicketSystem.Domain.Common;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class SqliteTicketRepository : ITicketRepository
{
    private readonly TicketDbContext dbContext;

    public SqliteTicketRepository(TicketDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddAsync(Ticket ticket)
    {
        await this.dbContext.Tickets.AddAsync(ticket);
        await this.dbContext.SaveChangesAsync();
    }

    public async Task<Ticket?> GetByIdAsync(TicketId id)
    {
        return await this.dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    }
}

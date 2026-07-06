using Microsoft.EntityFrameworkCore;
using TicketSystem.Domain.Common;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class SqliteNotificationRepository : INotificationRepository
{
    private readonly TicketDbContext dbContext;

    public SqliteNotificationRepository(TicketDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddAsync(Notification notification)
    {
        await this.dbContext.Notifications.AddAsync(notification);
        await this.dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        this.dbContext.Notifications.Update(notification);
        await this.dbContext.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetByTicketIdAsync(TicketId ticketId)
    {
        return await this.dbContext.Notifications
            .Where(n => n.TicketId == ticketId)
            .ToListAsync();
    }
}

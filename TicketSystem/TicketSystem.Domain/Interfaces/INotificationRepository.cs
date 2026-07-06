using TicketSystem.Domain.Common;
using TicketSystem.Domain.Models;

namespace TicketSystem.Domain.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);

    Task UpdateAsync(Notification notification);

    Task<List<Notification>> GetByTicketIdAsync(TicketId ticketId);
}

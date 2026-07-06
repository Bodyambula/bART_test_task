using Microsoft.EntityFrameworkCore;
using TicketSystem.Domain.Common;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Persistence;

public class TicketDbContext : DbContext
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public TicketDbContext(DbContextOptions<TicketDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasConversion(
                    id => id.Value,
                    value => new TicketId(value))
                .IsRequired();

            entity.Property(t => t.Title)
                .IsRequired();

            entity.Property(t => t.Priority)
                .HasConversion<string>();
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id)
                .HasConversion(
                    id => id.Value,
                    value => new NotificationId(value))
                .IsRequired();

            entity.Property(n => n.TicketId)
                .HasConversion(
                    id => id.Value,
                    value => new TicketId(value))
                .IsRequired();

            entity.Property(n => n.Channel)
                .HasConversion<string>();

            entity.Property(n => n.Status)
                .HasConversion<string>();
        });
    }
}

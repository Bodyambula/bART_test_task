// <copyright file="Ticket.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using TicketSystem.Domain.Enums;

namespace TicketSystem.Domain.Models;

public class Ticket
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Priority Priority { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

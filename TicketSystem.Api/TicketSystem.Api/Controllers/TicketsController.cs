using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Services;
using TicketSystem.Domain.Common;
using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository ticketRepository;
    private readonly INotificationRepository notificationRepository;
    private readonly NotificationService notificationService;

    public TicketsController(
        ITicketRepository ticketRepository,
        INotificationRepository notificationRepository,
        NotificationService notificationService)
    {
        this.ticketRepository = ticketRepository;
        this.notificationRepository = notificationRepository;
        this.notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var ticket = new Ticket(
            TicketId.New(),
            request.Title,
            request.Description,
            request.Priority,
            DateTimeOffset.UtcNow);

        await this.ticketRepository.AddAsync(ticket);
        await this.notificationService.CreatePendingNotificationsForTicketAsync(ticket.Id);

        var response = await this.GetTicketDetailsResponseAsync(ticket);
        return this.CreatedAtAction(nameof(this.GetTicket), new { id = ticket.Id.Value }, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(TicketId id)
    {
        var ticket = await this.ticketRepository.GetByIdAsync(id);
        if (ticket == null)
        {
            return this.NotFound(new { Message = $"Ticket with ID {id} not found." });
        }

        var response = await this.GetTicketDetailsResponseAsync(ticket);
        return this.Ok(response);
    }

    [HttpPost("{id}/notify")]
    public async Task<IActionResult> Notify(TicketId id)
    {
        var ticket = await this.ticketRepository.GetByIdAsync(id);
        if (ticket == null)
        {
            return this.NotFound(new { Message = $"Ticket with ID {id} not found." });
        }

        await this.notificationService.SendPendingOrFailedNotificationsAsync(id);

        var response = await this.GetTicketDetailsResponseAsync(ticket);
        return this.Ok(response);
    }

    private async Task<TicketDetailsResponse> GetTicketDetailsResponseAsync(Ticket ticket)
    {
        var notifications = await this.notificationRepository.GetByTicketIdAsync(ticket.Id);

        return new TicketDetailsResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Priority.ToString(),
            ticket.CreatedAt,
            notifications.Select(n => new NotificationResponse(
                n.Id,
                n.Channel.ToString(),
                n.Status.ToString(),
                n.Attempts,
                n.LastError,
                n.CreatedAt)).ToList());
    }
}

public record CreateTicketRequest(
    [property: Required]
    [property: MinLength(5, ErrorMessage = "Title must be at least 5 characters long.")]
    string Title,
    string? Description,
    Priority Priority);

public record TicketDetailsResponse(
    TicketId Id,
    string Title,
    string? Description,
    string Priority,
    DateTimeOffset CreatedAt,
    List<NotificationResponse> Notifications);

public record NotificationResponse(
    NotificationId Id,
    string Channel,
    string Status,
    int Attempts,
    string? LastError,
    DateTimeOffset CreatedAt);

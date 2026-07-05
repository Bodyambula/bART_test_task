using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Services;
using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;

namespace TicketSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository _ticketRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly NotificationService _notificationService;

    public TicketsController(
        ITicketRepository ticketRepository,
        INotificationRepository notificationRepository,
        NotificationService notificationService)
    {
        _ticketRepository = ticketRepository;
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _ticketRepository.AddAsync(ticket);
        await _notificationService.CreatePendingNotificationsForTicketAsync(ticket.Id);

        var response = await GetTicketDetailsResponseAsync(ticket);
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(Guid id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
        {
            return NotFound(new { Message = $"Ticket with ID {id} not found." });
        }

        var response = await GetTicketDetailsResponseAsync(ticket);
        return Ok(response);
    }

    [HttpPost("{id}/notify")]
    public async Task<IActionResult> Notify(Guid id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
        {
            return NotFound(new { Message = $"Ticket with ID {id} not found." });
        }

        await _notificationService.SendPendingOrFailedNotificationsAsync(id);

        var response = await GetTicketDetailsResponseAsync(ticket);
        return Ok(response);
    }

    private async Task<TicketDetailsResponse> GetTicketDetailsResponseAsync(Ticket ticket)
    {
        var notifications = await _notificationRepository.GetByTicketIdAsync(ticket.Id);
        
        return new TicketDetailsResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority.ToString(),
            CreatedAt = ticket.CreatedAt,
            Notifications = notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Channel = n.Channel.ToString(),
                Status = n.Status.ToString(),
                Attempts = n.Attempts,
                LastError = n.LastError,
                CreatedAt = n.CreatedAt
            }).ToList()
        };
    }
}

public class CreateTicketRequest
{
    [Required]
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters long.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Priority Priority { get; set; }
}

public class TicketDetailsResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public List<NotificationResponse> Notifications { get; set; } = new();
}

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

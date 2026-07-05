using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Controllers;
using TicketSystem.Api.Persistence;
using TicketSystem.Api.Services;
using TicketSystem.Domain.Enums;
using TicketSystem.Domain.Interfaces;
using TicketSystem.Domain.Models;
using Xunit;

namespace TicketSystem.Tests;

public class TicketTests
{
    private readonly InMemoryTicketRepository ticketRepository;
    private readonly InMemoryNotificationRepository notificationRepository;

    public TicketTests()
    {
        this.ticketRepository = new InMemoryTicketRepository();
        this.notificationRepository = new InMemoryNotificationRepository();
    }

    [Fact]
    public async Task CreateTicket_CreatesThreeNotificationsInPending()
    {
        // Arrange
        var senders = new List<INotificationSender>
        {
            new EmailNotificationSender(),
            new SmsNotificationSender(),
            new PushNotificationSender(),
        };
        var notificationService = new NotificationService(this.notificationRepository, this.ticketRepository, senders);
        var controller = new TicketsController(this.ticketRepository, this.notificationRepository, notificationService);

        var request = new CreateTicketRequest
        {
            Title = "Valid Ticket Title",
            Description = "A valid test description",
            Priority = Priority.High,
        };

        // Act
        var result = await controller.CreateTicket(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<TicketDetailsResponse>(createdResult.Value);

        Assert.NotNull(response);
        Assert.Equal(request.Title, response.Title);

        var notifications = await this.notificationRepository.GetByTicketIdAsync(response.Id);
        Assert.Equal(3, notifications.Count);
        Assert.All(notifications, n => Assert.Equal(NotificationStatus.Pending, n.Status));
        Assert.All(notifications, n => Assert.Equal(0, n.Attempts));
        Assert.All(notifications, n => Assert.Null(n.LastError));

        // Check that all channels (Email, SMS, Push) are present
        Assert.Contains(notifications, n => n.Channel == NotificationChannel.Email);
        Assert.Contains(notifications, n => n.Channel == NotificationChannel.Sms);
        Assert.Contains(notifications, n => n.Channel == NotificationChannel.Push);
    }

    [Fact]
    public async Task Notify_SendsAllAndUpdatesStatusesCorrectly()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = Priority.Medium,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await this.ticketRepository.AddAsync(ticket);

        var senders = new List<INotificationSender>
        {
            new EmailNotificationSender(),
            new SmsNotificationSender(),
            new PushNotificationSender(),
        };
        var notificationService = new NotificationService(this.notificationRepository, this.ticketRepository, senders);
        await notificationService.CreatePendingNotificationsForTicketAsync(ticket.Id);

        var controller = new TicketsController(this.ticketRepository, this.notificationRepository, notificationService);

        // Act
        var result = await controller.Notify(ticket.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TicketDetailsResponse>(okResult.Value);

        Assert.Equal(3, response.Notifications.Count);
        Assert.All(response.Notifications, n => Assert.Equal("Sent", n.Status));
        Assert.All(response.Notifications, n => Assert.Equal(1, n.Attempts));
        Assert.All(response.Notifications, n => Assert.Null(n.LastError));
    }

    [Fact]
    public async Task Retry_IncrementsAttemptsAndStopsAfterThree()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Fail Ticket",
            Priority = Priority.Low,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await this.ticketRepository.AddAsync(ticket);

        // Create mock senders that throw exception to trigger retry policy failure
        var failingSender = new FailingNotificationSender(NotificationChannel.Email, "Network timed out");
        var senders = new List<INotificationSender> { failingSender };

        var notificationService = new NotificationService(this.notificationRepository, this.ticketRepository, senders);

        // Add 1 Pending notification for Email
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Channel = NotificationChannel.Email,
            Status = NotificationStatus.Pending,
            Attempts = 0,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await this.notificationRepository.AddAsync(notification);

        // Act & Assert: Call Send 4 times
        // Call 1
        await notificationService.SendPendingOrFailedNotificationsAsync(ticket.Id);
        var updated = (await this.notificationRepository.GetByTicketIdAsync(ticket.Id)).First();
        Assert.Equal(1, updated.Attempts);
        Assert.Equal(NotificationStatus.Failed, updated.Status);
        Assert.Equal("Network timed out", updated.LastError);

        // Call 2
        await notificationService.SendPendingOrFailedNotificationsAsync(ticket.Id);
        updated = (await this.notificationRepository.GetByTicketIdAsync(ticket.Id)).First();
        Assert.Equal(2, updated.Attempts);
        Assert.Equal(NotificationStatus.Failed, updated.Status);

        // Call 3
        await notificationService.SendPendingOrFailedNotificationsAsync(ticket.Id);
        updated = (await this.notificationRepository.GetByTicketIdAsync(ticket.Id)).First();
        Assert.Equal(3, updated.Attempts);
        Assert.Equal(NotificationStatus.Failed, updated.Status);

        // Call 4: Attempts reached 3, should not retry again
        await notificationService.SendPendingOrFailedNotificationsAsync(ticket.Id);
        updated = (await this.notificationRepository.GetByTicketIdAsync(ticket.Id)).First();
        Assert.Equal(3, updated.Attempts); // Stays at 3, no more attempts executed
    }

    [Fact]
    public async Task Notify_IsIdempotent()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Idempotent Ticket",
            Priority = Priority.Medium,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await this.ticketRepository.AddAsync(ticket);

        // Implement a spy sender to count how many times it was called
        var spySender = new SpyNotificationSender(NotificationChannel.Email);
        var senders = new List<INotificationSender> { spySender };
        var notificationService = new NotificationService(this.notificationRepository, this.ticketRepository, senders);

        // Create 1 Pending email notification
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Channel = NotificationChannel.Email,
            Status = NotificationStatus.Pending,
            Attempts = 0,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await this.notificationRepository.AddAsync(notification);

        // Act
        // First Notify call
        await notificationService.SendPendingOrFailedNotificationsAsync(ticket.Id);
        var firstResult = (await this.notificationRepository.GetByTicketIdAsync(ticket.Id)).First();
        Assert.Equal(NotificationStatus.Sent, firstResult.Status);
        Assert.Equal(1, firstResult.Attempts);
        Assert.Equal(1, spySender.CallCount);

        // Second Notify call
        await notificationService.SendPendingOrFailedNotificationsAsync(ticket.Id);
        var secondResult = (await this.notificationRepository.GetByTicketIdAsync(ticket.Id)).First();

        // Assert: Status remains Sent, Attempts remains 1, SpySender was not called again
        Assert.Equal(NotificationStatus.Sent, secondResult.Status);
        Assert.Equal(1, secondResult.Attempts);
        Assert.Equal(1, spySender.CallCount);
    }

    [Fact]
    public async Task SendFailure_StoresErrorMessageInLastError()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Error Ticket",
            Priority = Priority.Medium,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await this.ticketRepository.AddAsync(ticket);

        var errorMsg = "SMS provider API is down.";
        var failingSender = new FailingNotificationSender(NotificationChannel.Sms, errorMsg);
        var notificationService = new NotificationService(this.notificationRepository, this.ticketRepository, new[] { failingSender });

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            Channel = NotificationChannel.Sms,
            Status = NotificationStatus.Pending,
            Attempts = 0,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await this.notificationRepository.AddAsync(notification);

        // Act
        await notificationService.SendPendingOrFailedNotificationsAsync(ticket.Id);

        // Assert
        var updated = (await this.notificationRepository.GetByTicketIdAsync(ticket.Id)).First();
        Assert.Equal(NotificationStatus.Failed, updated.Status);
        Assert.Equal(1, updated.Attempts);
        Assert.Equal(errorMsg, updated.LastError);
    }

    [Theory]
    [InlineData("Abc", false)] // Invalid: less than 5 characters
    [InlineData("Valid", true)] // Valid: exactly 5 characters
    [InlineData("Super Valid Title", true)] // Valid: more than 5 characters
    [InlineData("", false)] // Invalid: empty string
    [InlineData("    ", false)] // Invalid: whitespace (will be caught by MinLength as it's trimmed or evaluated as 4 chars if not trimmed)
    public void TicketTitleValidation_EnforcesMinLength(string title, bool expectedIsValid)
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = title,
            Description = "Description",
            Priority = Priority.Medium,
        };

        var context = new ValidationContext(request, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        // Assert
        Assert.Equal(expectedIsValid, isValid);
        if (!expectedIsValid)
        {
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        }
    }

    // Helper mock/spy classes for testing
    private class FailingNotificationSender : INotificationSender
    {
        private readonly string errorMessage;

        public NotificationChannel Channel { get; }

        public FailingNotificationSender(NotificationChannel channel, string errorMessage)
        {
            this.Channel = channel;
            this.errorMessage = errorMessage;
        }

        public Task SendAsync(Ticket ticket, Notification notification)
        {
            throw new Exception(this.errorMessage);
        }
    }

    private class SpyNotificationSender : INotificationSender
    {
        public NotificationChannel Channel { get; }

        public int CallCount { get; private set; }

        public SpyNotificationSender(NotificationChannel channel)
        {
            this.Channel = channel;
        }

        public Task SendAsync(Ticket ticket, Notification notification)
        {
            this.CallCount++;
            return Task.CompletedTask;
        }
    }
}

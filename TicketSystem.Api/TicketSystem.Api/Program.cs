// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using TicketSystem.Api.Persistence;
using TicketSystem.Api.Services;
using TicketSystem.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Register Repositories as Singletons for In-Memory storage
builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();
builder.Services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();

// Register Senders
builder.Services.AddTransient<INotificationSender, EmailNotificationSender>();
builder.Services.AddTransient<INotificationSender, SmsNotificationSender>();
builder.Services.AddTransient<INotificationSender, PushNotificationSender>();

// Register Services
builder.Services.AddScoped<NotificationService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

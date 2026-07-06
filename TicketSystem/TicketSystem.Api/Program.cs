using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<TicketDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ITicketRepository, SqliteTicketRepository>();
builder.Services.AddScoped<INotificationRepository, SqliteNotificationRepository>();

// Notification senders
builder.Services.AddTransient<INotificationSender, EmailNotificationSender>();
builder.Services.AddTransient<INotificationSender, SmsNotificationSender>();
builder.Services.AddTransient<INotificationSender, PushNotificationSender>();

// Application services
builder.Services.AddScoped<NotificationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Map strongly-typed IDs to UUID strings in Swagger
    options.MapType<TicketSystem.Domain.Common.TicketId>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "uuid"
    });
    options.MapType<TicketSystem.Domain.Common.NotificationId>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "uuid"
    });
});

var app = builder.Build();

// Ensure the database is created at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
    context.Database.EnsureCreated();
}

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

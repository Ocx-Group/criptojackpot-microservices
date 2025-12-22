using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Notification.Application;
using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Api.Consumers;
using CryptoJackpot.Notification.Api.Providers;
using CryptoJackpot.Notification.Data.Context;
using CryptoJackpot.Notification.Data.Repositories;
using CryptoJackpot.Notification.Domain.Interfaces;
using CryptoJackpot.Infra.IoC;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services.Configure<NotificationConfiguration>(builder.Configuration);

// Database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<INotificationLogRepository, NotificationLogRepository>();

// Providers
builder.Services.AddScoped<IEmailProvider, SmtpEmailProvider>();
builder.Services.AddSingleton<IEmailTemplateProvider, FileEmailTemplateProvider>();

// Application Layer (MediatR, Services, Handlers)
builder.Services.AddNotificationApplication();

// Register Infrastructure with Kafka
DependencyContainer.RegisterServicesWithKafka(
    builder.Services, 
    builder.Configuration,
    configureKafka: rider =>
    {
        // Register Kafka consumers
        rider.AddConsumer<UserRegisteredConsumer>();
        rider.AddConsumer<PasswordResetRequestedConsumer>();
        rider.AddConsumer<ReferralCreatedConsumer>();

        // Configure Kafka topic endpoints
        rider.UsingKafka((context, kafka) =>
        {
            kafka.Host(builder.Configuration["Kafka:Host"] ?? "localhost:9092");

            // Topic: user-registered -> sends email confirmation
            kafka.TopicEndpoint<UserRegisteredEvent>("user-registered", "notification-group", e =>
            {
                e.ConfigureConsumer<UserRegisteredConsumer>(context);
            });

            // Topic: password-reset-requested -> sends password reset email
            kafka.TopicEndpoint<PasswordResetRequestedEvent>("password-reset-requested", "notification-group", e =>
            {
                e.ConfigureConsumer<PasswordResetRequestedConsumer>(context);
            });

            // Topic: referral-created -> sends referral notification email
            kafka.TopicEndpoint<ReferralCreatedEvent>("referral-created", "notification-group", e =>
            {
                e.ConfigureConsumer<ReferralCreatedConsumer>(context);
            });
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

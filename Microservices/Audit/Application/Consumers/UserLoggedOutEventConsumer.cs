using CryptoJackpot.Audit.Domain.Enums;
using CryptoJackpot.Audit.Domain.Interfaces;
using CryptoJackpot.Audit.Domain.Models;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Audit.Application.Consumers;

/// <summary>
/// Consumes UserLoggedOutEvent from Identity microservice and creates audit logs.
/// </summary>
public class UserLoggedOutEventConsumer : IConsumer<UserLoggedOutEvent>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<UserLoggedOutEventConsumer> _logger;

    public UserLoggedOutEventConsumer(
        IAuditLogRepository repository,
        ILogger<UserLoggedOutEventConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserLoggedOutEvent> context)
    {
        var message = context.Message;

        _logger.LogDebug(
            "Received UserLoggedOutEvent for user {UserId} ({Email})",
            message.UserId,
            message.Email);

        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = message.LogoutTime,
                EventType = AuditEventType.UserLogout,
                Source = AuditSource.Identity,
                Status = AuditStatus.Success,
                CorrelationId = message.CorrelationId,
                Username = message.UserName,
                Action = "UserLogout",
                Description = $"User '{message.UserName}' ({message.Email}) logged out successfully",
                ResourceType = "User",
                ResourceId = message.UserId.ToString(),
                Request = new AuditRequestInfo
                {
                    IpAddress = message.IpAddress,
                    UserAgent = message.UserAgent
                },
                Metadata = new MongoDB.Bson.BsonDocument
                {
                    { "email", message.Email },
                    { "userName", message.UserName },
                    { "logoutTime", message.LogoutTime.ToString("O") }
                }
            };

            await _repository.CreateAsync(auditLog, context.CancellationToken);

            _logger.LogInformation(
                "Audit log created for UserLogout: User {UserId} ({UserName})",
                message.UserId,
                message.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create audit log for UserLogout: User {UserId}",
                message.UserId);
        }
    }
}


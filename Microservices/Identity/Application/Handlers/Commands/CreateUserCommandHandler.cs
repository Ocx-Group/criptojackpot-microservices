using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResultResponse<UserDto?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    private const long DefaultRoleId = 2;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEventBus eventBus,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<ResultResponse<UserDto?>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Email already registered");

        User? referrerUser = null;
        if (!string.IsNullOrEmpty(request.ReferralCode))
        {
            referrerUser = await _userRepository.GetBySecurityCodeAsync(request.ReferralCode);
            if (referrerUser is null)
                return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Invalid referral code");
        }

        var user = CreateUser(request);
        var createdUser = await _userRepository.CreateAsync(user);

        if (referrerUser != null)
            await HandleReferral(referrerUser, createdUser, request.ReferralCode!);

        await PublishUserRegisteredEvent(createdUser);

        return ResultResponse<UserDto?>.Created(createdUser.ToDto());
    }

    private User CreateUser(CreateUserCommand request) => new()
    {
        Email = request.Email,
        Password = _passwordHasher.Hash(request.Password),
        Name = request.Name,
        LastName = request.LastName,
        Phone = request.Phone,
        CountryId = request.CountryId ?? 1,
        StatePlace = string.Empty,
        City = string.Empty,
        SecurityCode = Guid.NewGuid().ToString(),
        Status = false,
        RoleId = DefaultRoleId
    };

    private async Task HandleReferral(User referrer, User referred, string referralCode)
    {
        referred.ReferredBy = new UserReferral
        {
            ReferrerId = referrer.Id,
            ReferredId = referred.Id,
            UsedSecurityCode = referralCode
        };
        await _userRepository.UpdateAsync(referred);
        await PublishReferralCreatedEvent(referrer, referred, referralCode);
    }

    private async Task PublishUserRegisteredEvent(User user)
    {
        try
        {
            await _eventBus.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                ConfirmationToken = user.SecurityCode!
            });
            _logger.LogInformation("UserRegisteredEvent published for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredEvent for user {UserId}", user.Id);
        }
    }

    private async Task PublishReferralCreatedEvent(User referrer, User referred, string referralCode)
    {
        try
        {
            await _eventBus.Publish(new ReferralCreatedEvent
            {
                ReferrerEmail = referrer.Email,
                ReferrerName = referrer.Name,
                ReferrerLastName = referrer.LastName,
                ReferredName = referred.Name,
                ReferredLastName = referred.LastName,
                ReferralCode = referralCode
            });
            _logger.LogInformation("ReferralCreatedEvent published for referrer {ReferrerId}", referrer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish ReferralCreatedEvent for referrer {ReferrerId}", referrer.Id);
        }
    }
}
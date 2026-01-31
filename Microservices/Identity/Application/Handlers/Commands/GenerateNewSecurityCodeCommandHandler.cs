using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Resends the email verification via Keycloak.
/// </summary>
public class GenerateNewSecurityCodeCommandHandler : IRequestHandler<GenerateNewSecurityCodeCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly IMapper _mapper;

    public GenerateNewSecurityCodeCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GenerateNewSecurityCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        if (user.Status)
            return Result.Fail<UserDto>(new BadRequestError("Email already verified"));

        if (string.IsNullOrEmpty(user.KeycloakId))
            return Result.Fail<UserDto>(new BadRequestError("User not linked to authentication service"));

        // Resend verification email via Keycloak
        await _keycloakAdminService.SendVerificationEmailAsync(user.KeycloakId, cancellationToken);

        return Result.Ok(_mapper.Map<UserDto>(user));
    }
}


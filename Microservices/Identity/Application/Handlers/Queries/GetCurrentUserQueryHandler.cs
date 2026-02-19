using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(
        IUserRepository userRepository,
        IStorageService storageService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.UserGuid);

        if (user is null)
            return Result.Fail<UserDto>(new NotFoundError("User not found"));

        var userDto = _mapper.Map<UserDto>(user);

        userDto.ImagePath = _storageService.GetImageUrl(userDto.ImagePath);

        return Result.Ok(userDto);
    }
}

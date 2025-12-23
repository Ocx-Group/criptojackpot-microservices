using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ResultResponse<UserDto?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;

    public GetUserByIdQueryHandler(
        IUserRepository userRepository,
        IStorageService storageService)
    {
        _userRepository = userRepository;
        _storageService = storageService;
    }

    public async Task<ResultResponse<UserDto?>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user is null)
            return ResultResponse<UserDto?>.Failure(ErrorType.NotFound, "User not found");

        var userDto = user.ToDto();
        
        // Generate presigned URL for image if exists
        if (!string.IsNullOrEmpty(userDto.ImagePath))
            userDto.ImagePath = _storageService.GetPresignedUrl(userDto.ImagePath);

        return ResultResponse<UserDto?>.Ok(userDto);
    }
}

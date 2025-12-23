using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class UpdateUserImageCommandHandler : IRequestHandler<UpdateUserImageCommand, ResultResponse<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;

    public UpdateUserImageCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IStorageService storageService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task<ResultResponse<UserDto>> Handle(
        UpdateUserImageCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user is null)
            return ResultResponse<UserDto>.Failure(ErrorType.NotFound, "User not found");

        // Update the image path with the storage key
        user.ImagePath = request.StorageKey;
        
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = user.ToDto();
        
        // Return the presigned URL for immediate use
        userDto.ImagePath = _storageService.GetPresignedUrl(request.StorageKey);

        return ResultResponse<UserDto>.Ok(userDto);
    }
}


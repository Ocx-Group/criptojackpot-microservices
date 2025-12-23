using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ResultResponse<IEnumerable<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;

    public GetAllUsersQueryHandler(
        IUserRepository userRepository,
        IStorageService storageService)
    {
        _userRepository = userRepository;
        _storageService = storageService;
    }

    public async Task<ResultResponse<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(request.ExcludeUserId);
        var userDtos = users.Select(u =>
        {
            var dto = u.ToDto();
            if (!string.IsNullOrEmpty(dto.ImagePath))
                dto.ImagePath = _storageService.GetPresignedUrl(dto.ImagePath);
            return dto;
        });

        return ResultResponse<IEnumerable<UserDto>>.Ok(userDtos);
    }
}
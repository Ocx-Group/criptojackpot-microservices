using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, Result<UserStatsDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserStatsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserStatsDto>> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        var totalUsers = await _userRepository.CountAsync();
        var usersThisMonth = await _userRepository.CountByDateRangeAsync(startOfThisMonth, now);
        var usersLastMonth = await _userRepository.CountByDateRangeAsync(startOfLastMonth, startOfThisMonth);

        var percentageChange = usersLastMonth > 0
            ? Math.Round((decimal)(usersThisMonth - usersLastMonth) / usersLastMonth * 100, 1)
            : usersThisMonth > 0 ? 100m : 0m;

        return Result.Ok(new UserStatsDto
        {
            TotalUsers = totalUsers,
            UsersThisMonth = usersThisMonth,
            UsersLastMonth = usersLastMonth,
            PercentageChange = percentageChange
        });
    }
}

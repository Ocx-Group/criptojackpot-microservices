using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.Queries;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Handlers.Queries;

public class GetOrderStatsQueryHandler : IRequestHandler<GetOrderStatsQuery, Result<OrderStatsDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IOrderRepository _orderRepository;

    public GetOrderStatsQueryHandler(
        ITicketRepository ticketRepository,
        IOrderRepository orderRepository)
    {
        _ticketRepository = ticketRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderStatsDto>> Handle(GetOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        // Tickets sold
        var totalTickets = await _ticketRepository.CountAsync();
        var ticketsThisMonth = await _ticketRepository.CountAsync(startOfThisMonth, now);
        var ticketsLastMonth = await _ticketRepository.CountAsync(startOfLastMonth, startOfThisMonth);
        var ticketsPctChange = ticketsLastMonth > 0
            ? Math.Round((decimal)(ticketsThisMonth - ticketsLastMonth) / ticketsLastMonth * 100, 1)
            : ticketsThisMonth > 0 ? 100m : 0m;

        // Revenue
        var totalRevenue = await _ticketRepository.SumRevenueAsync();
        var revenueThisMonth = await _ticketRepository.SumRevenueAsync(startOfThisMonth, now);
        var revenueLastMonth = await _ticketRepository.SumRevenueAsync(startOfLastMonth, startOfThisMonth);
        var revenuePctChange = revenueLastMonth > 0
            ? Math.Round((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100, 1)
            : revenueThisMonth > 0 ? 100m : 0m;

        // Conversion rate (completed orders / total orders)
        var completedThisMonth = await _orderRepository.CountCompletedAsync(startOfThisMonth, now);
        var totalOrdersThisMonth = await _orderRepository.CountAllAsync(startOfThisMonth, now);
        var conversionRate = totalOrdersThisMonth > 0
            ? Math.Round((decimal)completedThisMonth / totalOrdersThisMonth * 100, 1)
            : 0m;

        var completedLastMonth = await _orderRepository.CountCompletedAsync(startOfLastMonth, startOfThisMonth);
        var totalOrdersLastMonth = await _orderRepository.CountAllAsync(startOfLastMonth, startOfThisMonth);
        var conversionRateLastMonth = totalOrdersLastMonth > 0
            ? Math.Round((decimal)completedLastMonth / totalOrdersLastMonth * 100, 1)
            : 0m;

        var conversionChange = conversionRateLastMonth > 0
            ? Math.Round(conversionRate - conversionRateLastMonth, 1)
            : 0m;

        return Result.Ok(new OrderStatsDto
        {
            TotalTicketsSold = totalTickets,
            TicketsThisMonth = ticketsThisMonth,
            TicketsLastMonth = ticketsLastMonth,
            TicketsPercentageChange = ticketsPctChange,
            TotalRevenue = totalRevenue,
            RevenueThisMonth = revenueThisMonth,
            RevenueLastMonth = revenueLastMonth,
            RevenuePercentageChange = revenuePctChange,
            ConversionRate = conversionRate,
            ConversionRateLastMonth = conversionRateLastMonth,
            ConversionRateChange = conversionChange
        });
    }
}

using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class GenerateUploadUrlCommand : IRequest<ResultResponse<UploadUrlDto>>
{
    public long UserId { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public int ExpirationMinutes { get; set; } = 15;
}
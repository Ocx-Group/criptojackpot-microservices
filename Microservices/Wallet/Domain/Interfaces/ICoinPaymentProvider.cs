using CryptoJackpot.Domain.Core.Responses;

namespace CryptoJackpot.Wallet.Domain.Interfaces;

public interface ICoinPaymentProvider
{
    Task<RestResponse> CallApiAsync(
        string command, 
        SortedList<string, string>? parms = null,
        CancellationToken cancellationToken = default);
}
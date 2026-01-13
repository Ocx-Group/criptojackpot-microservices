namespace CryptoJackpot.Domain.Core.Responses.Errors;

public class ConflictError(string message) : ApplicationError(message, 409);


namespace CryptoJackpot.Identity.Application.DTOs;

public enum ErrorType
{
    None = 0,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500
}

public enum SuccessType
{
    Ok = 200,
    Created = 201,
    NoContent = 204
}

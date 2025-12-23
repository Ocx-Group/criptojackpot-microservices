namespace CryptoJackpot.Identity.Application.Requests;

public class GenerateUploadUrlRequest
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public int ExpirationMinutes { get; set; } = 15;
}


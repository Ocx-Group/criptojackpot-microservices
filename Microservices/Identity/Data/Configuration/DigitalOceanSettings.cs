namespace CryptoJackpot.Identity.Data.Configuration;

public class DigitalOceanSettings
{
    public string Endpoint { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string BucketName { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
}


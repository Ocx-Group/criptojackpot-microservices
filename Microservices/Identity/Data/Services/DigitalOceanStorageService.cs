using Amazon.S3;
using Amazon.S3.Model;
using CryptoJackpot.Identity.Data.Configuration;
using CryptoJackpot.Identity.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Data.Services;

public class DigitalOceanStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly DigitalOceanSettings _settings;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    public DigitalOceanStorageService(IOptions<DigitalOceanSettings> settings)
    {
        _settings = settings.Value;

        var config = new AmazonS3Config
        {
            ServiceURL = _settings.Endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = _settings.Region
        };

        _s3Client = new AmazonS3Client(
            _settings.AccessKey, 
            _settings.SecretKey, 
            config);
    }

    public (string Url, string Key) GeneratePresignedUploadUrl(
        long userId, 
        string fileName, 
        string contentType, 
        int expirationMinutes = 15)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomSuffix = Guid.NewGuid().ToString("N")[..8];
        var key = $"profile-photos/{userId}/user-{userId}-{timestamp}-{randomSuffix}{extension}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            ContentType = contentType
        };

        var url = _s3Client.GetPreSignedURL(request);
        return (url, key);
    }

    public string GetPresignedUrl(string key, int expirationMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public bool IsValidFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && AllowedExtensions.Contains(extension);
    }
}



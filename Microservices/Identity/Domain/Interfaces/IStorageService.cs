namespace CryptoJackpot.Identity.Domain.Interfaces;

/// <summary>
/// Service for cloud storage operations (DigitalOcean Spaces/S3)
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Generates a presigned URL for uploading a file
    /// </summary>
    (string Url, string Key) GeneratePresignedUploadUrl(
        long userId, 
        string fileName, 
        string contentType, 
        int expirationMinutes = 15);

    /// <summary>
    /// Generates a presigned URL for downloading/viewing a file
    /// </summary>
    string GetPresignedUrl(string key, int expirationMinutes = 60);

    /// <summary>
    /// Gets the image URL - returns presigned URL for internal paths, or the original URL for external URLs (Google, etc.)
    /// </summary>
    string? GetImageUrl(string? imagePath, int expirationMinutes = 60);

    /// <summary>
    /// Validates if a file extension is allowed
    /// </summary>
    bool IsValidFileExtension(string fileName);
}

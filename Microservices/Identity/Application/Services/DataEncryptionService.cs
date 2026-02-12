using CryptoJackpot.Identity.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Encrypts sensitive data using ASP.NET Core Data Protection.
/// Keys are managed via IDataProtectionProvider (configured in IoC).
/// In production, keys should be persisted to a secure store (Redis, Azure KeyVault, etc.)
/// </summary>
public class DataEncryptionService : IDataEncryptionService
{
    private const string Purpose = "CryptoJackpot.Identity.SensitiveData.v1";
    
    private readonly IDataProtector _protector;
    private readonly ILogger<DataEncryptionService> _logger;

    public DataEncryptionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<DataEncryptionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        return _protector.Protect(plainText);
    }

    public string? Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return null;

        try
        {
            return _protector.Unprotect(encryptedText);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt data. Key rotation may have occurred.");
            return null;
        }
    }

    public string? EncryptIfNotNull(string? plainText)
    {
        return string.IsNullOrEmpty(plainText) ? null : Encrypt(plainText);
    }

    public string? DecryptIfNotNull(string? encryptedText)
    {
        return string.IsNullOrEmpty(encryptedText) ? null : Decrypt(encryptedText);
    }
}


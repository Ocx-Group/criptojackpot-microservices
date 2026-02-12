namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for encrypting/decrypting sensitive data at rest.
/// Uses ASP.NET Core Data Protection for key management.
/// </summary>
public interface IDataEncryptionService
{
    /// <summary>
    /// Encrypts a plain text value for storage.
    /// </summary>
    /// <param name="plainText">The value to encrypt</param>
    /// <returns>Base64-encoded encrypted string</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted value.
    /// </summary>
    /// <param name="encryptedText">Base64-encoded encrypted string</param>
    /// <returns>Original plain text, or null if decryption fails</returns>
    string? Decrypt(string encryptedText);

    /// <summary>
    /// Encrypts a value, returning null if input is null.
    /// </summary>
    string? EncryptIfNotNull(string? plainText);

    /// <summary>
    /// Decrypts a value, returning null if input is null.
    /// </summary>
    string? DecryptIfNotNull(string? encryptedText);
}


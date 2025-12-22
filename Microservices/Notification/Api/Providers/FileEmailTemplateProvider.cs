using CryptoJackpot.Notification.Application.Interfaces;

namespace CryptoJackpot.Notification.Api.Providers;

/// <summary>
/// Loads email templates from HTML files in the Templates folder.
/// </summary>
public class FileEmailTemplateProvider : IEmailTemplateProvider
{
    private readonly string _templatesPath;
    private readonly ILogger<FileEmailTemplateProvider> _logger;
    private readonly Dictionary<string, string> _templateCache = new();

    public FileEmailTemplateProvider(ILogger<FileEmailTemplateProvider> logger)
    {
        _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        _logger = logger;
    }

    public async Task<string?> GetTemplateAsync(string templateName)
    {
        // Check cache first
        if (_templateCache.TryGetValue(templateName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var filePath = Path.Combine(_templatesPath, $"{templateName}.html");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Template file not found: {FilePath}", filePath);
            return null;
        }

        try
        {
            var template = await File.ReadAllTextAsync(filePath);
            _templateCache[templateName] = template;
            _logger.LogDebug("Template loaded from file: {TemplateName}", templateName);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading template file: {FilePath}", filePath);
            return null;
        }
    }
}


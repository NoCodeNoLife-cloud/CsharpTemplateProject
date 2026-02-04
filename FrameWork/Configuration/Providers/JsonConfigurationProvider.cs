using System.Text.Json;
using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Interfaces;

namespace CommonFramework.Configuration.Providers;

/// <summary>
/// JSON configuration provider implementation
/// </summary>
public sealed class JsonConfigurationProvider : IConfigurationProvider
{
    /// <summary>
    /// Gets the name of the configuration provider
    /// </summary>
    public string Name => "JSON";

    /// <summary>
    /// Check if the provider can handle the specified configuration source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Whether it can be handled</returns>
    public bool CanHandleSource(string source)
    {
        return source.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Load configuration data from JSON file
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="JsonException">Thrown when JSON parsing fails</exception>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        ValidateFileExists(source);

        var jsonContent = File.ReadAllText(source);
        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

        return jsonObject ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Validates that the configuration file exists
    /// </summary>
    /// <param name="source">The file path to validate</param>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when file does not exist</exception>
    private static void ValidateFileExists(string source)
    {
        if (!File.Exists(source))
            throw new ConfigurationFileNotFoundException(source);
    }
}
namespace CommonFramework.Configuration.Interfaces;

/// <summary>
/// Unified configuration service interface
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Register configuration provider
    /// </summary>
    /// <param name="provider">Configuration provider instance</param>
    void RegisterProvider(IConfigurationProvider provider);

    /// <summary>
    /// Load configuration
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    Dictionary<string, object> LoadConfiguration(string source);

    /// <summary>
    /// Get configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Configuration value</returns>
    T? GetValue<T>(string key, T defaultValue = default(T));

    /// <summary>
    /// Set configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Configuration value</param>
    void SetValue(string key, object value);

    /// <summary>
    /// Refresh configuration
    /// </summary>
    void Refresh();

    /// <summary>
    /// Check if configuration key exists
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <returns>Whether key exists</returns>
    bool ContainsKey(string key);

    /// <summary>
    /// Get all configuration keys
    /// </summary>
    /// <returns>Collection of configuration keys</returns>
    IEnumerable<string> GetAllKeys();
}
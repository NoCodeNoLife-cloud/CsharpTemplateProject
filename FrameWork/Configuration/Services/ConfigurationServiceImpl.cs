using CommonFramework.Configuration.Interfaces;
using CommonFramework.Configuration.Providers;
using LoggingService.Enums;
using LoggingService.Services;

namespace CommonFramework.Configuration.Services;

/// <summary>
/// Unified configuration service implementation
/// Uses strategy pattern to manage different configuration providers
/// </summary>
public class ConfigurationServiceImpl : IConfigurationService
{
    private static readonly Lazy<ConfigurationServiceImpl> Instance = new(() => new ConfigurationServiceImpl());
    private readonly List<IConfigurationProvider> _providers;
    private readonly Dictionary<string, object> _configurationCache;
    private readonly Lock _lockObject = new();

    private ConfigurationServiceImpl()
    {
        _providers = new List<IConfigurationProvider>();
        _configurationCache = new Dictionary<string, object>();

        // Register all built-in providers by default
        RegisterBuiltInProviders();
    }

    /// <summary>
    /// Get singleton instance
    /// </summary>
    public static ConfigurationServiceImpl InstanceVal => Instance.Value;

    /// <summary>
    /// Register built-in configuration providers
    /// </summary>
    private void RegisterBuiltInProviders()
    {
        RegisterProvider(new JsonConfigurationProvider());
        RegisterProvider(new XmlConfigurationProvider());
        RegisterProvider(new YamlConfigurationProvider());

        LoggingServiceImpl.InstanceVal.LogDebug("Registered built-in configuration providers: JSON, XML, YAML");
    }

    /// <summary>
    /// Register configuration provider
    /// </summary>
    /// <param name="provider">Configuration provider instance</param>
    public void RegisterProvider(IConfigurationProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        lock (_lockObject)
        {
            if (!_providers.Any(p => p.Name.Equals(provider.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _providers.Add(provider);
                LoggingServiceImpl.InstanceVal.LogDebug($"Registered configuration provider: {provider.Name}");
            }
        }
    }

    /// <summary>
    /// Load configuration
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Configuration source path cannot be empty", nameof(source));

        var provider = FindSuitableProvider(source);
        if (provider == null)
            throw new InvalidOperationException($"No suitable provider found for configuration source '{source}'");

        lock (_lockObject)
        {
            try
            {
                var configData = provider.LoadConfiguration(source);
                MergeConfiguration(configData);

                LoggingServiceImpl.InstanceVal.LogInformation($"Successfully loaded configuration from {provider.Name}: {source}");
                return new Dictionary<string, object>(_configurationCache);
            }
            catch (Exception ex)
            {
                LoggingServiceImpl.InstanceVal.LogError($"Failed to load configuration: {source}", ex);
                throw;
            }
        }
    }

    /// <summary>
    /// Find suitable configuration provider
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration provider instance</returns>
    private IConfigurationProvider? FindSuitableProvider(string source)
    {
        return _providers.FirstOrDefault(p => p.CanHandleSource(source));
    }

    /// <summary>
    /// Merge configuration data into cache
    /// </summary>
    /// <param name="newConfig">New configuration data</param>
    private void MergeConfiguration(Dictionary<string, object> newConfig)
    {
        foreach (var kvp in newConfig)
        {
            _configurationCache[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Get configuration value
    /// </summary>
    /// <typeparam name="T">Return value type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Configuration value</returns>
    public T? GetValue<T>(string key, T defaultValue = default(T))
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(key));

        lock (_lockObject)
        {
            if (_configurationCache.TryGetValue(key, out var value))
            {
                try
                {
                    return ConvertValue<T>(value);
                }
                catch (Exception ex)
                {
                    // Use Log method with Warning level and exception
                    LoggingServiceImpl.InstanceVal.Log(LogLevel.Warning, $"Configuration value conversion failed: {key} -> {typeof(T).Name}", ex);
                }
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"Configuration key not found, returning default value: {key}");
            return defaultValue;
        }
    }

    /// <summary>
    /// Convert configuration value type
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="value">Original value</param>
    /// <returns>Converted value</returns>
    private T? ConvertValue<T>(object value)
    {
        if (value is T directValue)
            return directValue;

        var targetType = typeof(T);

        // Handle nullable types
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (string.IsNullOrEmpty(value.ToString()))
                return default(T);

            targetType = Nullable.GetUnderlyingType(targetType);
        }

        // String type direct conversion
        if (targetType == typeof(string))
            return (T)(object)value.ToString()!;

        // Enum type
        if (targetType is { IsEnum: true })
            return (T)Enum.Parse(targetType, value.ToString() ?? throw new InvalidOperationException(), true);

        // Other types use Convert.ChangeType
        return (T)Convert.ChangeType(value, targetType ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Set configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Configuration value</param>
    public void SetValue(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(key));

        lock (_lockObject)
        {
            _configurationCache[key] = value;
            LoggingServiceImpl.InstanceVal.LogDebug($"Set configuration value: {key} = {value}");
        }
    }

    /// <summary>
    /// Refresh configuration
    /// </summary>
    public void Refresh()
    {
        lock (_lockObject)
        {
            _configurationCache.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("Configuration cache cleared");
        }
    }

    /// <summary>
    /// Get all configuration keys
    /// </summary>
    /// <returns>Configuration key collection</returns>
    public IEnumerable<string> GetAllKeys()
    {
        lock (_lockObject)
        {
            return _configurationCache.Keys.ToList();
        }
    }

    /// <summary>
    /// Check if specified configuration key exists
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <returns>Whether it exists</returns>
    public bool ContainsKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        lock (_lockObject)
        {
            return _configurationCache.ContainsKey(key);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Validates that the provider is not null
    /// </summary>
    /// <param name="provider">The provider to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when provider is null</exception>
    private static void _validateProviderNotNull(IConfigurationProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider), "Configuration provider cannot be null");
    }

    /// <summary>
    /// Validates that the source is not empty
    /// </summary>
    /// <param name="source">The source to validate</param>
    /// <exception cref="ArgumentException">Thrown when source is null or whitespace</exception>
    private static void _validateSourceNotEmpty(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Configuration source path cannot be empty", nameof(source));
    }

    /// <summary>
    /// Validates that the key is not empty
    /// </summary>
    /// <param name="key">The key to validate</param>
    /// <exception cref="ArgumentException">Thrown when key is null or whitespace</exception>
    private static void _validateKeyNotEmpty(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(key));
    }

    /// <summary>
    /// Validates that a suitable provider was found
    /// </summary>
    /// <param name="provider">The provider to validate</param>
    /// <param name="source">The source that was being processed</param>
    /// <exception cref="InvalidOperationException">Thrown when no suitable provider is found</exception>
    private static void _validateProviderFound(IConfigurationProvider provider, string source)
    {
        if (provider == null)
            throw new InvalidOperationException($"ConfigurationServiceImpl.FindSuitableProvider: No suitable provider found for configuration source '{source}'");
    }

    #endregion
}
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
        ArgumentNullException.ThrowIfNull(provider);

        lock (_lockObject)
        {
            if (_providers.Any(p => p.Name.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))) return;
            _providers.Add(provider);
            LoggingServiceImpl.InstanceVal.LogDebug($"Registered configuration provider: {provider.Name}");
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
                LoggingServiceImpl.InstanceVal.LogDebug($"Loaded {configData.Count} configuration entries from {source}");
                foreach (var kvp in configData)
                {
                    LoggingServiceImpl.InstanceVal.LogDebug($"  Key: '{kvp.Key}', Value: '{kvp.Value}' ({kvp.Value?.GetType().Name})");
                }
                
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
        LoggingServiceImpl.InstanceVal.LogDebug($"Merging {newConfig.Count} configuration entries into cache");
        foreach (var kvp in newConfig)
        {
            _configurationCache[kvp.Key] = kvp.Value;
            LoggingServiceImpl.InstanceVal.LogDebug($"  Added to cache: '{kvp.Key}' = '{kvp.Value}' ({kvp.Value?.GetType().Name})");
        }
        LoggingServiceImpl.InstanceVal.LogDebug($"Cache now contains {_configurationCache.Count} entries");
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
    private static T? ConvertValue<T>(object value)
    {
        if (value is T directValue)
            return directValue;

        var targetType = typeof(T);
        var stringValue = value.ToString();

        // Handle nullable types
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (string.IsNullOrEmpty(stringValue))
                return default(T);

            targetType = Nullable.GetUnderlyingType(targetType);
        }

        // String type direct conversion
        if (targetType == typeof(string))
            return (T)(object)stringValue!;

        // Handle numeric conversions from string with culture-invariant parsing
        if (targetType == typeof(int) && int.TryParse(stringValue, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var intValue))
            return (T)(object)intValue;
        
        if (targetType == typeof(double) && double.TryParse(stringValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
            return (T)(object)doubleValue;
        
        if (targetType == typeof(float) && float.TryParse(stringValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var floatValue))
            return (T)(object)floatValue;
        
        if (targetType == typeof(decimal) && decimal.TryParse(stringValue, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var decimalValue))
            return (T)(object)decimalValue;

        // Boolean conversion with flexible parsing
        if (targetType == typeof(bool))
        {
            if (bool.TryParse(stringValue, out var boolValue))
                return (T)(object)boolValue;
            
            // Handle case-insensitive "true"/"false" strings
            if (string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase))
                return (T)(object)true;
            if (string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase))
                return (T)(object)false;
        }

        // Enum type
        if (targetType is { IsEnum: true })
            return (T)Enum.Parse(targetType, stringValue ?? throw new InvalidOperationException(), true);

        // Other types use Convert.ChangeType as fallback
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
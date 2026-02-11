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

    // Constants for logging messages
    private static class LogMessages
    {
        public const string RegisteredProviders = "Registered built-in configuration providers: JSON, XML, YAML";
        public const string RegisteredProvider = "Registered configuration provider: {0}";
        public const string LoadedEntries = "Loaded {0} configuration entries from {1}";
        public const string EntryDetail = "  Key: '{0}', Value: '{1}' ({2})";
        public const string MergingEntries = "Merging {0} configuration entries into cache";
        public const string AddedToCache = "  Added to cache: '{0}' = '{1}' ({2})";
        public const string CacheSize = "Cache now contains {0} entries";
        public const string SuccessfullyLoaded = "Successfully loaded configuration from {0}: {1}";
        public const string FailedToLoad = "Failed to load configuration: {0}";
        public const string ConversionFailed = "Configuration value conversion failed: {0} -> {1}";
        public const string KeyNotFound = "Configuration key not found, returning default value: {0}";
        public const string SetValue = "Set configuration value: {0} = {1}";
        public const string CacheCleared = "Configuration cache cleared";
    }

    private ConfigurationServiceImpl()
    {
        _providers = [];
        _configurationCache = new Dictionary<string, object>();
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
        LoggingServiceImpl.InstanceVal.LogDebug(LogMessages.RegisteredProviders);
    }

    /// <summary>
    /// Register configuration provider
    /// </summary>
    /// <param name="provider">Configuration provider instance</param>
    /// <exception cref="ArgumentNullException">Thrown when provider is null</exception>
    public void RegisterProvider(IConfigurationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ValidateProviderUniqueness(provider);
    }

    /// <summary>
    /// Validates that the provider name is unique
    /// </summary>
    /// <param name="provider">Configuration provider to validate</param>
    private void ValidateProviderUniqueness(IConfigurationProvider provider)
    {
        lock (_lockObject)
        {
            if (_providers.Any(p => p.Name.Equals(provider.Name, StringComparison.OrdinalIgnoreCase)))
            {
                LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.RegisteredProvider, provider.Name));
                return;
            }

            _providers.Add(provider);
            LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.RegisteredProvider, provider.Name));
        }
    }

    /// <summary>
    /// Load configuration synchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when no suitable provider is found</exception>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        ValidateSource(source);
        var provider = FindSuitableProvider(source) ??
                       throw new InvalidOperationException($"No suitable provider found for configuration source '{source}'");

        return LoadConfigurationInternal(source, provider);
    }

    /// <summary>
    /// Load configuration asynchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when no suitable provider is found</exception>
    public async Task<Dictionary<string, object>> LoadConfigurationAsync(string source, CancellationToken cancellationToken = default)
    {
        ValidateSource(source);
        var provider = FindSuitableProvider(source) ??
                       throw new InvalidOperationException($"No suitable provider found for configuration source '{source}'");

        return await LoadConfigurationInternalAsync(source, provider, cancellationToken);
    }

    /// <summary>
    /// Validates configuration source parameter
    /// </summary>
    /// <param name="source">Configuration source to validate</param>
    private static void ValidateSource(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Configuration source path cannot be empty", nameof(source));
    }

    /// <summary>
    /// Internal synchronous configuration loading implementation
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <param name="provider">Configuration provider</param>
    /// <returns>Configuration dictionary</returns>
    private Dictionary<string, object> LoadConfigurationInternal(string source, IConfigurationProvider provider)
    {
        lock (_lockObject)
        {
            try
            {
                var configData = provider.LoadConfiguration(source);
                LogConfigurationData(source, configData);
                MergeConfiguration(configData);
                LoggingServiceImpl.InstanceVal.LogInformation(string.Format(LogMessages.SuccessfullyLoaded, provider.Name, source));
                return new Dictionary<string, object>(_configurationCache);
            }
            catch (Exception ex)
            {
                LoggingServiceImpl.InstanceVal.LogError(string.Format(LogMessages.FailedToLoad, source), ex);
                throw;
            }
        }
    }

    /// <summary>
    /// Internal asynchronous configuration loading implementation
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <param name="provider">Configuration provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration dictionary</returns>
    private async Task<Dictionary<string, object>> LoadConfigurationInternalAsync(string source, IConfigurationProvider provider, CancellationToken cancellationToken)
    {
        // Get the configuration data first (this might involve async operations)
        Dictionary<string, object> configData;

        // Check if provider supports async loading
        if (provider is IAsyncConfigurationProvider asyncProvider)
        {
            configData = await asyncProvider.LoadConfigurationAsync(source, cancellationToken);
        }
        else
        {
            // Fallback to synchronous loading
            configData = provider.LoadConfiguration(source);
        }

        // Now acquire lock only for the cache operations
        lock (_lockObject)
        {
            try
            {
                LogConfigurationData(source, configData);
                MergeConfiguration(configData);
                LoggingServiceImpl.InstanceVal.LogInformation(string.Format(LogMessages.SuccessfullyLoaded, provider.Name, source));
                return new Dictionary<string, object>(_configurationCache);
            }
            catch (Exception ex)
            {
                LoggingServiceImpl.InstanceVal.LogError(string.Format(LogMessages.FailedToLoad, source), ex);
                throw;
            }
        }
    }

    /// <summary>
    /// Logs configuration data details
    /// </summary>
    /// <param name="source">Configuration source</param>
    /// <param name="configData">Configuration data</param>
    private void LogConfigurationData(string source, Dictionary<string, object> configData)
    {
        LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.LoadedEntries, configData.Count, source));
        foreach (var kvp in configData)
        {
            LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.EntryDetail, kvp.Key, kvp.Value, kvp.Value.GetType().Name));
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
        LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.MergingEntries, newConfig.Count));

        foreach (var kvp in newConfig)
        {
            _configurationCache[kvp.Key] = kvp.Value;
            LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.AddedToCache, kvp.Key, kvp.Value, kvp.Value.GetType().Name));
        }

        LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.CacheSize, _configurationCache.Count));
    }

    /// <summary>
    /// Get configuration value
    /// </summary>
    /// <typeparam name="T">Return value type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Configuration value</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null</exception>
    /// <exception cref="ArgumentException">Thrown when key is empty or whitespace</exception>
    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        ValidateKey(key);
        return GetValueInternal<T>(key, defaultValue ?? throw new ArgumentNullException(nameof(defaultValue)));
    }

    /// <summary>
    /// Validates configuration key parameter
    /// </summary>
    /// <param name="key">Configuration key to validate</param>
    private static void ValidateKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be empty", nameof(key));
    }

    /// <summary>
    /// Internal value retrieval implementation
    /// </summary>
    /// <typeparam name="T">Return value type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Configuration value</returns>
    private T? GetValueInternal<T>(string key, T defaultValue)
    {
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
                    LoggingServiceImpl.InstanceVal.Log(LogLevel.Warning, string.Format(LogMessages.ConversionFailed, key, typeof(T).Name), ex);
                }
            }

            LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.KeyNotFound, key));
            return defaultValue;
        }
    }

    /// <summary>
    /// Convert configuration value type with improved separation of concerns
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
        if (!IsNullableType(targetType, out var underlyingType))
            return targetType.Name switch
            {
                nameof(String) => (T)(object)stringValue!,
                nameof(Int32) => ConvertToInt<T>(stringValue),
                nameof(Double) => ConvertToDouble<T>(stringValue),
                nameof(Single) => ConvertToFloat<T>(stringValue),
                nameof(Decimal) => ConvertToDecimal<T>(stringValue),
                nameof(Boolean) => ConvertToBoolean<T>(stringValue),
                _ => HandleSpecialTypes<T>(targetType, stringValue, value)
            };
        if (string.IsNullOrEmpty(stringValue))
            return default;
        targetType = underlyingType;

        return targetType?.Name switch
        {
            nameof(String) => (T)(object)stringValue,
            nameof(Int32) => ConvertToInt<T>(stringValue),
            nameof(Double) => ConvertToDouble<T>(stringValue),
            nameof(Single) => ConvertToFloat<T>(stringValue),
            nameof(Decimal) => ConvertToDecimal<T>(stringValue),
            nameof(Boolean) => ConvertToBoolean<T>(stringValue),
            _ => HandleSpecialTypes<T>(targetType, stringValue, value)
        };
    }

    /// <summary>
    /// Checks if type is nullable and gets underlying type
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <param name="underlyingType">Underlying type if nullable</param>
    /// <returns>True if type is nullable</returns>
    private static bool IsNullableType(Type type, out Type? underlyingType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return true;
        }

        underlyingType = null;
        return false;
    }

    /// <summary>
    /// Converts string to integer
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="value">String value</param>
    /// <returns>Converted integer</returns>
    private static T ConvertToInt<T>(string? value)
    {
        return int.TryParse(value, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? (T)(object)result
            : throw new InvalidCastException($"Cannot convert '{value}' to int");
    }

    /// <summary>
    /// Converts string to double
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="value">String value</param>
    /// <returns>Converted double</returns>
    private static T ConvertToDouble<T>(string? value)
    {
        return double.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? (T)(object)result
            : throw new InvalidCastException($"Cannot convert '{value}' to double");
    }

    /// <summary>
    /// Converts string to float
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="value">String value</param>
    /// <returns>Converted float</returns>
    private static T ConvertToFloat<T>(string? value)
    {
        return float.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? (T)(object)result
            : throw new InvalidCastException($"Cannot convert '{value}' to float");
    }

    /// <summary>
    /// Converts string to decimal
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="value">String value</param>
    /// <returns>Converted decimal</returns>
    private static T ConvertToDecimal<T>(string? value)
    {
        return decimal.TryParse(value, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? (T)(object)result
            : throw new InvalidCastException($"Cannot convert '{value}' to decimal");
    }

    /// <summary>
    /// Converts string to boolean
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="value">String value</param>
    /// <returns>Converted boolean</returns>
    private static T ConvertToBoolean<T>(string? value)
    {
        if (bool.TryParse(value, out var result))
            return (T)(object)result;

        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            return (T)(object)true;
        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            return (T)(object)false;

        throw new InvalidCastException($"Cannot convert '{value}' to boolean");
    }

    /// <summary>
    /// Handles special types like enums and fallback conversion
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <param name="targetType">Target type</param>
    /// <param name="stringValue">String value</param>
    /// <param name="originalValue">Original value</param>
    /// <returns>Converted value</returns>
    private static T HandleSpecialTypes<T>(Type? targetType, string? stringValue, object originalValue)
    {
        if (targetType is { IsEnum: true })
            return (T)Enum.Parse(targetType, stringValue ?? throw new InvalidOperationException(), true);

        // Other types use Convert.ChangeType as fallback
        return (T)Convert.ChangeType(originalValue, targetType ?? throw new ArgumentNullException(nameof(targetType)));
    }

    /// <summary>
    /// Set configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Configuration value</param>
    /// <exception cref="ArgumentNullException">Thrown when key is null</exception>
    /// <exception cref="ArgumentException">Thrown when key is empty or whitespace</exception>
    public void SetValue(string key, object value)
    {
        ValidateKey(key);
        SetValueInternal(key, value);
    }

    /// <summary>
    /// Internal value setting implementation
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Configuration value</param>
    private void SetValueInternal(string key, object value)
    {
        lock (_lockObject)
        {
            _configurationCache[key] = value;
            LoggingServiceImpl.InstanceVal.LogDebug(string.Format(LogMessages.SetValue, key, value));
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
            LoggingServiceImpl.InstanceVal.LogInformation(LogMessages.CacheCleared);
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
}

/// <summary>
/// Interface for async configuration providers
/// </summary>
public interface IAsyncConfigurationProvider
{
    /// <summary>
    /// Load configuration data asynchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration dictionary</returns>
    Task<Dictionary<string, object>> LoadConfigurationAsync(string source, CancellationToken cancellationToken = default);
}
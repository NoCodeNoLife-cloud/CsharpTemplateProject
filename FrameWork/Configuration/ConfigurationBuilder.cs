using CommonFramework.Configuration.Interfaces;
using CommonFramework.Configuration.Services;

namespace CommonFramework.Configuration;

/// <summary>
/// Configuration builder, provides fluent API to configure and use configuration service
/// </summary>
public sealed class ConfigurationBuilder
{
    private readonly IConfigurationService _configurationService;

    /// <summary>
    /// Initializes a new instance of the ConfigurationBuilder class
    /// </summary>
    public ConfigurationBuilder() : this(ConfigurationServiceImpl.InstanceVal)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationBuilder class with specified service
    /// </summary>
    /// <param name="configurationService">Configuration service instance</param>
    /// <exception cref="ArgumentNullException">Thrown when configurationService is null</exception>
    public ConfigurationBuilder(IConfigurationService configurationService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Add custom configuration provider
    /// </summary>
    /// <param name="provider">Configuration provider</param>
    /// <returns>Configuration builder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when provider is null</exception>
    public ConfigurationBuilder AddProvider(IConfigurationProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
            
        _configurationService.RegisterProvider(provider);
        return this;
    }

    /// <summary>
    /// Load configuration from source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration builder instance</returns>
    /// <exception cref="ArgumentException">Thrown when source is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when no suitable provider is found</exception>
    public ConfigurationBuilder LoadFrom(string source)
    {
        if (string.IsNullOrEmpty(source))
            throw new ArgumentException("Source cannot be null or empty", nameof(source));
            
        _configurationService.LoadConfiguration(source);
        return this;
    }

    /// <summary>
    /// Batch load multiple configuration sources
    /// </summary>
    /// <param name="sources">Configuration source path array</param>
    /// <returns>Configuration builder instance</returns>
    /// <exception cref="ArgumentException">Thrown when any source is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when no suitable provider is found for any source</exception>
    public ConfigurationBuilder LoadFrom(params string[] sources)
    {
        if (sources == null)
            throw new ArgumentNullException(nameof(sources));

        foreach (var source in sources)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty", nameof(sources));
                
            _configurationService.LoadConfiguration(source);
        }

        return this;
    }

    /// <summary>
    /// Build and get configuration service instance
    /// </summary>
    /// <returns>Configuration service instance</returns>
    public IConfigurationService Build()
    {
        return _configurationService;
    }

    /// <summary>
    /// Create default configuration builder
    /// </summary>
    /// <returns>Configuration builder instance</returns>
    public static ConfigurationBuilder CreateDefault()
    {
        return new ConfigurationBuilder();
    }

    /// <summary>
    /// Create configuration builder with custom service
    /// </summary>
    /// <param name="configurationService">Custom configuration service</param>
    /// <returns>Configuration builder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when configurationService is null</exception>
    public static ConfigurationBuilder CreateWithService(IConfigurationService configurationService)
    {
        return new ConfigurationBuilder(configurationService);
    }
}
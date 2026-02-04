namespace CommonFramework.Configuration.Interfaces;

/// <summary>
/// Configuration provider interface, defining the basic contract for configuration loading
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Gets the name of the configuration provider
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Load configuration data
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    Dictionary<string, object> LoadConfiguration(string source);

    /// <summary>
    /// Check if the provider can handle the specified configuration source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Whether it can be handled</returns>
    bool CanHandleSource(string source);
}
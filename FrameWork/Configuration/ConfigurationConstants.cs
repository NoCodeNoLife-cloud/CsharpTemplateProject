namespace CommonFramework.Configuration;

/// <summary>
/// Constants for configuration providers
/// </summary>
public static class ConfigurationConstants
{
    /// <summary>
    /// JSON provider name
    /// </summary>
    public const string JsonProviderName = "JSON";

    /// <summary>
    /// XML provider name
    /// </summary>
    public const string XmlProviderName = "XML";

    /// <summary>
    /// YAML provider name
    /// </summary>
    public const string YamlProviderName = "YAML";

    /// <summary>
    /// JSON file extension
    /// </summary>
    public const string JsonExtension = ".json";

    /// <summary>
    /// XML file extension
    /// </summary>
    public const string XmlExtension = ".xml";

    /// <summary>
    /// YAML file extensions
    /// </summary>
    public static readonly string[] YamlExtensions = [".yaml", ".yml"];

    /// <summary>
    /// All supported configuration file extensions
    /// </summary>
    public static readonly string[] AllExtensions = [JsonExtension, XmlExtension, ..YamlExtensions];
}
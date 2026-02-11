using System.Runtime.Serialization;

namespace CommonFramework.Configuration.Exceptions;

/// <summary>
/// Configuration file not found exception
/// </summary>
[Serializable]
public sealed class ConfigurationFileNotFoundException : ConfigurationException
{
    /// <summary>
    /// Gets the file path that was not found
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Initializes a new instance of the ConfigurationFileNotFoundException class with the file path
    /// </summary>
    /// <param name="filePath">The path of the configuration file that was not found</param>
    public ConfigurationFileNotFoundException(string filePath)
        : base($"ConfigurationFileNotFoundException..ctor: Configuration file not found at path '{filePath}'")
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationFileNotFoundException class with the file path and inner exception
    /// </summary>
    /// <param name="filePath">The path of the configuration file that was not found</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConfigurationFileNotFoundException(string filePath, Exception innerException)
        : base($"ConfigurationFileNotFoundException..ctor: Configuration file not found at path '{filePath}'", innerException)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationFileNotFoundException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    private ConfigurationFileNotFoundException(SerializationInfo info, StreamingContext context)
    {
        FilePath = info.GetString(nameof(FilePath)) ?? string.Empty;
    }
}
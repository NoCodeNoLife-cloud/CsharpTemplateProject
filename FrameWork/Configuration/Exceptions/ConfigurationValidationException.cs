using System.Runtime.Serialization;

namespace CommonFramework.Configuration.Exceptions;

/// <summary>
/// Configuration validation exception
/// </summary>
[Serializable]
public sealed class ConfigurationValidationException : ConfigurationException
{
    /// <summary>
    /// Gets the configuration key that failed validation
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the expected type for the configuration value
    /// </summary>
    public Type? ExpectedType { get; }

    /// <summary>
    /// Initializes a new instance of the ConfigurationValidationException class with key and expected type
    /// </summary>
    /// <param name="key">The configuration key that failed validation</param>
    /// <param name="expectedType">The expected type for the configuration value</param>
    public ConfigurationValidationException(string key, Type expectedType)
        : base($"ConfigurationValidationException..ctor: Configuration validation failed for key '{key}'. Expected type: {expectedType.Name}")
    {
        Key = key;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationValidationException class with key, expected type and inner exception
    /// </summary>
    /// <param name="key">The configuration key that failed validation</param>
    /// <param name="expectedType">The expected type for the configuration value</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConfigurationValidationException(string key, Type expectedType, Exception innerException)
        : base($"ConfigurationValidationException..ctor: Configuration validation failed for key '{key}'. Expected type: {expectedType.Name}", innerException)
    {
        Key = key;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationValidationException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    private ConfigurationValidationException(SerializationInfo info, StreamingContext context)
    {
        Key = info.GetString(nameof(Key)) ?? string.Empty;
        ExpectedType = Type.GetType(info.GetString(nameof(ExpectedType)) ?? typeof(object).FullName!);
    }
}
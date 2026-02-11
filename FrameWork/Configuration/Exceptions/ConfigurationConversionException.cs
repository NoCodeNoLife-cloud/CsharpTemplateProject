using System.Runtime.Serialization;

namespace CommonFramework.Configuration.Exceptions;

/// <summary>
/// Configuration conversion exception
/// </summary>
[Serializable]
public sealed class ConfigurationConversionException : ConfigurationException
{
    /// <summary>
    /// Gets the configuration key that failed conversion
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the source type of the configuration value
    /// </summary>
    public Type? SourceType { get; }

    /// <summary>
    /// Gets the target type for conversion
    /// </summary>
    public Type? TargetType { get; }

    /// <summary>
    /// Initializes a new instance of the ConfigurationConversionException class with key, source type and target type
    /// </summary>
    /// <param name="key">The configuration key that failed conversion</param>
    /// <param name="sourceType">The source type of the configuration value</param>
    /// <param name="targetType">The target type for conversion</param>
    public ConfigurationConversionException(string key, Type sourceType, Type targetType)
        : base($"ConfigurationConversionException..ctor: Cannot convert configuration value for key '{key}' from {sourceType.Name} to {targetType.Name}")
    {
        Key = key;
        SourceType = sourceType;
        TargetType = targetType;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationConversionException class with key, source type, target type and inner exception
    /// </summary>
    /// <param name="key">The configuration key that failed conversion</param>
    /// <param name="sourceType">The source type of the configuration value</param>
    /// <param name="targetType">The target type for conversion</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConfigurationConversionException(string key, Type sourceType, Type targetType, Exception innerException)
        : base($"ConfigurationConversionException..ctor: Cannot convert configuration value for key '{key}' from {sourceType.Name} to {targetType.Name}", innerException)
    {
        Key = key;
        SourceType = sourceType;
        TargetType = targetType;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigurationConversionException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    private ConfigurationConversionException(SerializationInfo info, StreamingContext context)
    {
        Key = info.GetString(nameof(Key)) ?? string.Empty;
        SourceType = Type.GetType(info.GetString(nameof(SourceType)) ?? typeof(object).FullName!);
        TargetType = Type.GetType(info.GetString(nameof(TargetType)) ?? typeof(object).FullName!);
    }
}
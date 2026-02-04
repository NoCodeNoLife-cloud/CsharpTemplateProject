using System.Runtime.Serialization;

namespace CommonFramework.Configuration.Exceptions;

/// <summary>
/// Configuration provider not found exception
/// </summary>
[Serializable]
public sealed class ProviderNotFoundException : ConfigurationException
{
    /// <summary>
    /// Gets the name of the provider that was not found
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    /// Initializes a new instance of the ProviderNotFoundException class with the provider name
    /// </summary>
    /// <param name="providerName">The name of the provider that was not found</param>
    public ProviderNotFoundException(string providerName)
        : base($"ProviderNotFoundException..ctor: Configuration provider '{providerName}' not found")
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Initializes a new instance of the ProviderNotFoundException class with the provider name and inner exception
    /// </summary>
    /// <param name="providerName">The name of the provider that was not found</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ProviderNotFoundException(string providerName, Exception innerException)
        : base($"ProviderNotFoundException..ctor: Configuration provider '{providerName}' not found", innerException)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Initializes a new instance of the ProviderNotFoundException class with serialized data
    /// </summary>
    /// <param name="info">The SerializationInfo that holds the serialized object data</param>
    /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
    private ProviderNotFoundException(SerializationInfo info, StreamingContext context)
    {
        ProviderName = info.GetString(nameof(ProviderName)) ?? string.Empty;
    }
}
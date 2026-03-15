namespace Common.Models.Responses;

/// <summary>
/// Connection test response model
/// </summary>
public class ConnectionTestResponse
{
    /// <summary>
    /// Whether connection is successful
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>
    /// Test timestamp
    /// </summary>
    public DateTime TestedAt { get; init; }
}
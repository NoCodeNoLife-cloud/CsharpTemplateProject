namespace Common.Models;

/// <summary>
/// Connection test response model
/// </summary>
public class ConnectionTestResponse
{
    /// <summary>
    /// Whether connection is successful
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;
    
    /// <summary>
    /// Test timestamp
    /// </summary>
    public DateTime TestedAt { get; set; }
}

namespace Common.Models;

/// <summary>
/// API response wrapper
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }
}

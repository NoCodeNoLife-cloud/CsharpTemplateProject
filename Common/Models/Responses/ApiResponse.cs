namespace Common.Models.Responses;

/// <summary>
/// API response wrapper
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; init; }
}
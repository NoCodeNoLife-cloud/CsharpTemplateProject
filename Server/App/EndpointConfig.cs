namespace Server.App;

/// <summary>
/// Endpoint configuration model
/// </summary>
public class EndpointConfig
{
    /// <summary>
    /// Endpoint path (e.g., "/api/test")
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP Method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string Method { get; set; } = "GET";
    
    /// <summary>
    /// Response content type
    /// </summary>
    public string ContentType { get; set; } = "text/plain";
    
    /// <summary>
    /// Response message or template
    /// </summary>
    public string Response { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether authentication is required
    /// </summary>
    public bool RequireAuth { get; set; } = false;
    
    /// <summary>
    /// Custom handler logic (optional, for complex scenarios)
    /// </summary>
    public Func<Microsoft.AspNetCore.Http.HttpContext, Task>? Handler { get; set; }
}
namespace Common.Models;

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Whether login was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// User ID
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Username
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// User status/role
    /// </summary>
    public string? Status { get; set; }
}

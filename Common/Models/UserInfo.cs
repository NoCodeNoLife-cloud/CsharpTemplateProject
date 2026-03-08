namespace Common.Models;

/// <summary>
/// User info model for Client usage
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Username
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// User priority/role
    /// </summary>
    public string? Priority { get; set; }
}

namespace Common.Models;

/// <summary>
/// User info response model
/// </summary>
public class UserInfoResponse
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

namespace Common.Models.Responses;

/// <summary>
/// User info response model
/// </summary>
public class UserInfoResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public int Id { get; init; }
    
    /// <summary>
    /// Username
    /// </summary>
    public string? Username { get; init; }
    
    /// <summary>
    /// User priority/role
    /// </summary>
    public string? Priority { get; init; }
}
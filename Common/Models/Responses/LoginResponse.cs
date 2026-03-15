namespace Common.Models.Responses;

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
    public int? UserId { get; init; }

    /// <summary>
    /// Username
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// User status/role
    /// </summary>
    public string? Status { get; init; }
}
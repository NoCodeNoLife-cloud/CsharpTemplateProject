using System.ComponentModel.DataAnnotations;

namespace Common.Models;

/// <summary>
/// Register request DTO
/// </summary>
/// <param name="Username">Desired username</param>
/// <param name="Password">User password</param>
/// <param name="Priority">User priority/role (default: "user")</param>
public record RegisterRequest(
    [property: Required(ErrorMessage = "Username is required")]
    [property: StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    string Username,
    [property: Required(ErrorMessage = "Password is required")]
    [property: StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    string Password,
    [property: StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
    string Priority = "user"
);
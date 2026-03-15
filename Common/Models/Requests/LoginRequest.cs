using System.ComponentModel.DataAnnotations;

namespace Common.Models.Requests;

/// <summary>
/// Login request DTO
/// </summary>
public record LoginRequest(
    [property: Required(ErrorMessage = "Username is required")]
    [property: StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    string Username,
    [property: Required(ErrorMessage = "Password is required")]
    [property: StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    string Password
);
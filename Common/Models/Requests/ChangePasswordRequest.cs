using System.ComponentModel.DataAnnotations;

namespace Common.Models.Requests;

/// <summary>
/// Change password request DTO
/// </summary>
public record ChangePasswordRequest(
    [property: Required(ErrorMessage = "User ID is required")]
    int UserId,
    
    [property: Required(ErrorMessage = "Current password is required")]
    [property: StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    string CurrentPassword,
    
    [property: Required(ErrorMessage = "New password is required")]
    [property: StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters")]
    string NewPassword
);
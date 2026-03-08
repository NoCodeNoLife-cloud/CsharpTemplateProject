using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.Database.UserAuthentication;

/// <summary>
/// Local user authentication service for managing authentication state in Client application
/// </summary>
public static class UserAuthenticationService
{
    private static int? _currentUserId;
    private static string? _currentUsername;
    private static LoginStatus _currentUserStatus = LoginStatus.NotLoggedIn;

    /// <summary>
    /// Get current user ID
    /// </summary>
    public static int? CurrentUserId => _currentUserId;

    /// <summary>
    /// Get current username
    /// </summary>
    public static string? CurrentUsername => _currentUsername;

    /// <summary>
    /// Get current user login status
    /// </summary>
    public static LoginStatus CurrentUserStatus => _currentUserStatus;

    /// <summary>
    /// Set logged in user information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="username">Username</param>
    /// <param name="status">Login status string ("RegularUser", "Administrator", etc.)</param>
    public static void SetLoggedInUser(int userId, string username, string status)
    {
        _currentUserId = userId;
        _currentUsername = username;
        
        // Parse status string to LoginStatus enum
        _currentUserStatus = status.ToLower() switch
        {
            "regularuser" or "user" => LoginStatus.RegularUser,
            "administrator" or "admin" => LoginStatus.Administrator,
            "superadministrator" or "superadmin" => LoginStatus.SuperAdministrator,
            _ => LoginStatus.RegularUser // Default to RegularUser
        };
        
        LoggingFactory.Instance.LogDebug($"User set: ID={userId}, Username={username}, Status={_currentUserStatus}");
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    public static void Logout()
    {
        LoggingFactory.Instance.LogDebug($"Logging out user: {_currentUsername}");
        _currentUserId = null;
        _currentUsername = null;
        _currentUserStatus = LoginStatus.NotLoggedIn;
    }

    /// <summary>
    /// Check if user is logged in
    /// </summary>
    /// <returns>True if user is logged in</returns>
    public static bool IsUserLoggedIn()
    {
        return _currentUserStatus != LoginStatus.NotLoggedIn && _currentUserId.HasValue;
    }

    /// <summary>
    /// Check if current user is administrator
    /// </summary>
    /// <returns>True if user has administrator permissions</returns>
    public static bool IsUserAdministrator()
    {
        return _currentUserStatus == LoginStatus.Administrator || 
               _currentUserStatus == LoginStatus.SuperAdministrator;
    }

    /// <summary>
    /// Verify password against stored hash (placeholder for local validation if needed)
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="passwordHash">Stored password hash</param>
    /// <returns>True if password matches</returns>
    public static bool VerifyPassword(string password, string passwordHash)
    {
        // This is a placeholder - in HTTP API mode, password verification happens on server
        // If you need local validation, implement BCrypt or similar here
        LoggingFactory.Instance.LogWarning("VerifyPassword called but not implemented for HTTP API mode");
        return false;
    }

    /// <summary>
    /// Get user by ID from local cache (placeholder)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User info or null</returns>
    public static async Task<object?> GetUserByIdAsync(int userId)
    {
        // In HTTP API mode, this should call ServerAuthService.GetUserInfoAsync(userId)
        // This method is kept for backward compatibility
        LoggingFactory.Instance.LogWarning("GetUserByIdAsync called - use ServerAuthService instead");
        return await Task.FromResult<object?>(null);
    }
}

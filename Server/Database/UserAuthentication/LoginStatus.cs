namespace Server.Database.UserAuthentication;

/// <summary>
/// User login status enumeration
/// Represents the current login status and permission level of users
/// </summary>
public enum LoginStatus
{
    /// <summary>
    /// Not logged in status
    /// User has not been authenticated yet
    /// </summary>
    NotLoggedIn = 0,

    /// <summary>
    /// Regular user status
    /// Logged-in regular user with basic permissions
    /// </summary>
    RegularUser = 1,

    /// <summary>
    /// Administrator status
    /// Logged-in administrator user with management permissions
    /// </summary>
    Administrator = 2,

    /// <summary>
    /// Super administrator status
    /// Administrator with the highest permission level
    /// </summary>
    SuperAdministrator = 3
}

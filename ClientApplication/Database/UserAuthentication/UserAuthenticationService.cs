using MySqlConnector;
using ClientApplication.Database;
using LoggingService.Services;
using LoggingService.Enums;
using Sql.Exceptions;

namespace ClientApplication.Database.UserAuthentication;

/// <summary>
/// Service class for user authentication and login operations
/// </summary>
public static class UserAuthenticationService
{
    /// <summary>
    /// Authenticates a user by username and password
    /// </summary>
    /// <param name="username">Username to authenticate</param>
    /// <param name="password">Password to verify</param>
    /// <returns>Tuple containing (success, userId, username) - success indicates if authentication was successful</returns>
    public static async Task<(bool success, int? userId, string? username)> AuthenticateUserAsync(string username, string password)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Authenticating user '{username}' with provided password...");
            
            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();
            
            await using var cmd = new MySqlCommand(
                "SELECT id, username, password FROM `user` WHERE username = @username", connection);
            cmd.Parameters.AddWithValue("@username", username);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var userId = reader.GetInt32("id");
                var foundUsername = reader.GetString("username");
                var storedPassword = reader.GetString("password");
                
                // In a real application, you should hash the password and compare hashes
                // For this demo, we'll do direct comparison
                if (storedPassword == password)
                {
                    LoggingServiceImpl.InstanceVal.LogDebug($"User authentication successful: ID={userId}, Username={foundUsername}");
                    return (true, userId, foundUsername);
                }
                else
                {
                    LoggingServiceImpl.InstanceVal.LogDebug($"Password mismatch for user '{username}'");
                    return (false, null, null);
                }
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogDebug($"User '{username}' not found in database");
                return (false, null, null);
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to authenticate user '{username}': {ex.Message}");
            return (false, null, null);
        }
    }
}
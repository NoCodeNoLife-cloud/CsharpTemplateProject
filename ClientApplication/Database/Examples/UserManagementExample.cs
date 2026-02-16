using MySqlConnector;
using ClientApplication.Database;
using LoggingService.Services;
using LoggingService.Enums;
using Sql.Exceptions;

namespace ClientApplication.Database.Examples;

/// <summary>
/// Example usage of the database setup and user management
/// </summary>
public static class UserManagementExample
{
    /// <summary>
    /// Demonstrates adding a user to the database
    /// </summary>
    public static async Task<bool> AddUserAsync(string username, string password)
    {
        try
        {
            // First check if user already exists
            if (await UserExistsAsync(username))
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User '{username}' already exists, skipping insertion.");
                return true; // Return true since the user effectively exists
            }
            
            LoggingServiceImpl.InstanceVal.LogDebug($"Adding user '{username}' to database...");
            
            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();
            
            await using var cmd = new MySqlCommand(
                "INSERT INTO `user` (username, password) VALUES (@username, @password)", connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password); // In real application, hash the password!
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"Successfully added user '{username}'");
                return true;
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"No rows affected when adding user '{username}'");
                return false;
            }
        }

        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to add user '{username}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if a user already exists in the database
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <returns>True if user exists, false otherwise</returns>
    public static async Task<bool> UserExistsAsync(string username)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Checking if user '{username}' exists...");
            
            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();
            
            await using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM `user` WHERE username = @username", connection);
            cmd.Parameters.AddWithValue("@username", username);
            
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            var exists = count > 0;
            
            LoggingServiceImpl.InstanceVal.LogDebug($"User '{username}' {(exists ? "exists" : "does not exist")}");
            return exists;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to check if user '{username}' exists: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Demonstrates retrieving a user from the database
    /// </summary>
    public static async Task<(bool success, int? userId, string? username)> GetUserAsync(string username)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Retrieving user '{username}' from database...");
            
            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();
            
            await using var cmd = new MySqlCommand(
                "SELECT id, username FROM `user` WHERE username = @username", connection);
            cmd.Parameters.AddWithValue("@username", username);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var userId = reader.GetInt32("id");
                var foundUsername = reader.GetString("username");
                LoggingServiceImpl.InstanceVal.LogDebug($"Found user: ID={userId}, Username={foundUsername}");
                return (true, userId, foundUsername);
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogDebug($"User '{username}' not found");
                return (false, null, null);
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to retrieve user '{username}': {ex.Message}");
            return (false, null, null);
        }
    }

    /// <summary>
    /// Demonstrates listing all users
    /// </summary>
    public static async Task<List<(int id, string username)>> ListAllUsersAsync()
    {
        var users = new List<(int id, string username)>();
        
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Listing all users from database...");
            
            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();
            
            await using var cmd = new MySqlCommand("SELECT id, username FROM `user` ORDER BY id", connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32("id");
                var username = reader.GetString("username");
                users.Add((id, username));
            }
            
            LoggingServiceImpl.InstanceVal.LogDebug($"Found {users.Count} users in database");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to list users: {ex.Message}");
        }
        
        return users;
    }

    /// <summary>
    /// Demonstrates adding a user with duplicate checking
    /// </summary>
    public static async Task DemonstrateSafeUserInsertionAsync()
    {
        LoggingServiceImpl.InstanceVal.LogDebug("=== Demonstrating Safe User Insertion ===");
        
        // Try to add the same user multiple times
        var usernames = new[] { "testuser1", "testuser2", "testuser1" }; // Note: testuser1 appears twice
        
        foreach (var username in usernames)
        {
            var added = await AddUserAsync(username, $"password_{username}");
            LoggingServiceImpl.InstanceVal.LogInformation($"Attempt to add '{username}': {(added ? "Processed" : "Failed")}");
        }
        
        // Show all users
        var allUsers = await ListAllUsersAsync();
        LoggingServiceImpl.InstanceVal.LogInformation($"Total users in database: {allUsers.Count}");
        
        LoggingServiceImpl.InstanceVal.LogDebug("=== Safe User Insertion Demo Complete ===");
    }

}
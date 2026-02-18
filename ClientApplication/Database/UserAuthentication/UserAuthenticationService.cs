using MySqlConnector;
using ClientApplication.Database;
using ClientApplication.Database.Models;
using ClientApplication.Database.Services;
using LoggingService.Services;
using LoggingService.Enums;
using Sql.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace ClientApplication.Database.UserAuthentication;

/// <summary>
/// Service class for user authentication and login operations
/// </summary>
public static class UserAuthenticationService
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Pbkdf2Iterations = 10000;

    /// <summary>
    /// Authenticates a user by username and password using secure hashing
    /// </summary>
    /// <param name="username">Username to authenticate</param>
    /// <param name="password">Password to verify</param>
    /// <returns>Tuple containing (success, userId, username) - success indicates if authentication was successful</returns>
    [Obsolete("Obsolete")]
    public static async Task<(bool success, int? userId, string? username)> AuthenticateUserAsync(string username, string password)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Authenticating user '{username}' with provided password...");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT id, username, password_hash FROM `user` WHERE username = @username", connection);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var userId = reader.GetInt32("id");
                var foundUsername = reader.GetString("username");
                var storedHash = reader.GetString("password_hash");

                // Verify password using secure hash comparison
                if (VerifyPassword(password, storedHash))
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

    /// <summary>
    /// Hashes a password using PBKDF2 with salt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Base64 encoded hashed password</returns>
    [Obsolete("Obsolete")]
    public static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Stored hash</param>
    /// <returns>True if password matches hash</returns>
    [Obsolete("Obsolete")]
    public static bool VerifyPassword(string password, string hash)
    {
        var hashBytes = Convert.FromBase64String(hash);
        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var hashToCheck = pbkdf2.GetBytes(HashSize);

        for (var i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != hashToCheck[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Registers a new user with username and password
    /// </summary>
    /// <param name="username">Username for new account</param>
    /// <param name="password">Password for new account</param>
    /// <returns>Tuple containing (success, userId, errorMessage) - success indicates if registration was successful</returns>
    [Obsolete("Obsolete")]
    public static async Task<(bool success, int? userId, string? errorMessage)> RegisterUserAsync(string username, string password)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Attempting to register new user '{username}'...");

            // Use UserService for registration to leverage CRUD functionality
            var userService = new UserService();
            
            // First check if username already exists
            var existingUser = await userService.FindByUsernameAsync(username);
            if (existingUser != null)
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"Registration failed: Username '{username}' already exists");
                return (false, null, "Username already exists");
            }

            // Hash the password
            var hashedPassword = HashPassword(password);
            
            // Create user entity
            var newUser = new User(username, hashedPassword);
            
            // Save user using CRUD service
            var createdUser = await userService.CreateAsync(newUser);
            
            if (createdUser.Id > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User registration successful: ID={createdUser.Id}, Username={username}");
                return (true, createdUser.Id, null);
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogError($"Failed to insert user '{username}' into database");
                return (false, null, "Failed to create user account");
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to register user '{username}': {ex.Message}");
            return (false, null, $"Registration error: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates a user's password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newPassword">New password</param>
    /// <returns>True if update successful, false otherwise</returns>
    [Obsolete("Obsolete")]
    public static async Task<bool> UpdateUserPasswordAsync(int userId, string newPassword)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Updating password for user ID {userId}...");
            
            var userService = new UserService();
            var result = await userService.UpdatePasswordAsync(userId, newPassword);
            
            if (result)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"Password updated successfully for user ID {userId}");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"Failed to update password for user ID {userId}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to update password for user ID {userId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User entity if found, null otherwise</returns>
    [Obsolete("Obsolete")]
    public static async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Getting user by ID {userId}...");
            
            var userService = new UserService();
            var user = await userService.GetByIdAsync(userId);
            
            if (user != null)
            {
                LoggingServiceImpl.InstanceVal.LogDebug($"User found: {user}");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogDebug($"User with ID {userId} not found");
            }
            
            return user;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get user by ID {userId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>Collection of all users</returns>
    [Obsolete("Obsolete")]
    public static async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Getting all users...");
            
            var userService = new UserService();
            var users = await userService.GetAllAsync();
            
            LoggingServiceImpl.InstanceVal.LogDebug($"Retrieved {users.Count()} users");
            return users;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get all users: {ex.Message}");
            return Enumerable.Empty<User>();
        }
    }

    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if deletion successful, false otherwise</returns>
    [Obsolete("Obsolete")]
    public static async Task<bool> DeleteUserAsync(int userId)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Deleting user ID {userId}...");
            
            var userService = new UserService();
            var result = await userService.DeleteAsync(userId);
            
            if (result)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User deleted successfully: ID={userId}");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"Failed to delete user ID {userId}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to delete user ID {userId}: {ex.Message}");
            return false;
        }
    }




}
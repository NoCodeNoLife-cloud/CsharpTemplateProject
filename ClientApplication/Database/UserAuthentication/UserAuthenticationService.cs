using MySqlConnector;
using ClientApplication.Database;
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
}
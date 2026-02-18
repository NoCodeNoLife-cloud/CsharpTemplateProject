using MySqlConnector;
using Sql.Interfaces;
using ClientApplication.Database.Models;
using LoggingService.Services;
using LoggingService.Enums;
using System.Security.Cryptography;
using System.Text;

namespace ClientApplication.Database.Services;

/// <summary>
/// User service implementing CRUD operations for user management
/// </summary>
public class UserService : ICrudService<User, int>
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Pbkdf2Iterations = 10000;

    /// <summary>
    /// Creates a new user entity
    /// </summary>
    /// <param name="entity">The user entity to create</param>
    /// <returns>The created user entity</returns>
    public User Create(User entity)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Creating user '{entity.Username}'...");

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "INSERT INTO `user` (username, password_hash) VALUES (@username, @passwordHash)", 
                connection);
            
            cmd.Parameters.AddWithValue("@username", entity.Username);
            cmd.Parameters.AddWithValue("@passwordHash", entity.PasswordHash);

            var rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                // Get the newly created user ID
                using var getIdCmd = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
                entity.Id = Convert.ToInt32(getIdCmd.ExecuteScalar());
                
                LoggingServiceImpl.InstanceVal.LogInformation($"User created successfully: ID={entity.Id}, Username={entity.Username}");
                return entity;
            }
            
            throw new InvalidOperationException("Failed to create user");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to create user '{entity.Username}': {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously creates a new user entity
    /// </summary>
    /// <param name="entity">The user entity to create</param>
    /// <returns>Task containing the created user entity</returns>
    public async Task<User> CreateAsync(User entity)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Creating user '{entity.Username}' asynchronously...");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "INSERT INTO `user` (username, password_hash) VALUES (@username, @passwordHash)", 
                connection);
            
            cmd.Parameters.AddWithValue("@username", entity.Username);
            cmd.Parameters.AddWithValue("@passwordHash", entity.PasswordHash);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                // Get the newly created user ID
                await using var getIdCmd = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
                entity.Id = Convert.ToInt32(await getIdCmd.ExecuteScalarAsync());
                
                LoggingServiceImpl.InstanceVal.LogInformation($"User created successfully: ID={entity.Id}, Username={entity.Username}");
                return entity;
            }
            
            throw new InvalidOperationException("Failed to create user");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to create user '{entity.Username}': {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>The found user, or null if not found</returns>
    public User? GetById(int id)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Getting user by ID: {id}");

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "SELECT id, username, password_hash FROM `user` WHERE id = @id", 
                connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                
                LoggingServiceImpl.InstanceVal.LogDebug($"User found: {user}");
                return user;
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"User with ID {id} not found");
            return null;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get user by ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Task containing the found user, or null if not found</returns>
    public async Task<User?> GetByIdAsync(int id)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Getting user by ID: {id} asynchronously");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT id, username, password_hash FROM `user` WHERE id = @id", 
                connection);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                
                LoggingServiceImpl.InstanceVal.LogDebug($"User found: {user}");
                return user;
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"User with ID {id} not found");
            return null;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get user by ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>Collection of all users</returns>
    public IEnumerable<User> GetAll()
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Getting all users");

            var users = new List<User>();

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "SELECT id, username, password_hash FROM `user` ORDER BY id DESC", 
                connection);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                users.Add(user);
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"Retrieved {users.Count} users");
            return users;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get all users: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously gets all users
    /// </summary>
    /// <returns>Task containing collection of all users</returns>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Getting all users asynchronously");

            var users = new List<User>();

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT id, username, password_hash FROM `user` ORDER BY id DESC", 
                connection);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                users.Add(user);
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"Retrieved {users.Count} users");
            return users;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get all users: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user entity
    /// </summary>
    /// <param name="entity">The user entity to update</param>
    /// <returns>The updated user entity</returns>
    public User Update(User entity)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Updating user ID {entity.Id}");

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "UPDATE `user` SET username = @username, password_hash = @passwordHash WHERE id = @id", 
                connection);
            
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@username", entity.Username);
            cmd.Parameters.AddWithValue("@passwordHash", entity.PasswordHash);

            var rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User updated successfully: ID={entity.Id}, Username={entity.Username}");
                return entity;
            }
            
            throw new InvalidOperationException($"User with ID {entity.Id} not found");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to update user ID {entity.Id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously updates an existing user entity
    /// </summary>
    /// <param name="entity">The user entity to update</param>
    /// <returns>Task containing the updated user entity</returns>
    public async Task<User> UpdateAsync(User entity)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Updating user ID {entity.Id} asynchronously");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "UPDATE `user` SET username = @username, password_hash = @passwordHash WHERE id = @id", 
                connection);
            
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@username", entity.Username);
            cmd.Parameters.AddWithValue("@passwordHash", entity.PasswordHash);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User updated successfully: ID={entity.Id}, Username={entity.Username}");
                return entity;
            }
            
            throw new InvalidOperationException($"User with ID {entity.Id} not found");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to update user ID {entity.Id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    public bool Delete(int id)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Deleting user ID {id}");

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand("DELETE FROM `user` WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);

            var rowsAffected = cmd.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User deleted successfully: ID={id}");
                return true;
            }
            
            LoggingServiceImpl.InstanceVal.LogWarning($"User with ID {id} not found for deletion");
            return false;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to delete user ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously deletes a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Task containing true if deletion was successful, false otherwise</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Deleting user ID {id} asynchronously");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand("DELETE FROM `user` WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User deleted successfully: ID={id}");
                return true;
            }
            
            LoggingServiceImpl.InstanceVal.LogWarning($"User with ID {id} not found for deletion");
            return false;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to delete user ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    // Additional batch operations from ICrudService interface
    public IEnumerable<User> CreateBatch(IEnumerable<User> entities) => throw new NotImplementedException();
    public Task<IEnumerable<User>> CreateBatchAsync(IEnumerable<User> entities) => throw new NotImplementedException();
    public IEnumerable<User> UpdateBatch(IEnumerable<User> entities) => throw new NotImplementedException();
    public Task<IEnumerable<User>> UpdateBatchAsync(IEnumerable<User> entities) => throw new NotImplementedException();
    public int DeleteBatch(IEnumerable<int> ids) => throw new NotImplementedException();
    public Task<int> DeleteBatchAsync(IEnumerable<int> ids) => throw new NotImplementedException();

    /// <summary>
    /// Checks if a user exists by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if user exists, false otherwise</returns>
    public bool Exists(int id)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Checking if user ID {id} exists");

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM `user` WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to check if user ID {id} exists: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously checks if a user exists by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Task containing true if user exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Checking if user ID {id} exists asynchronously");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM `user` WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", id);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to check if user ID {id} exists: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets the count of users
    /// </summary>
    /// <returns>Total number of users</returns>
    public int Count()
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Getting user count");

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM `user`", connection);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            
            LoggingServiceImpl.InstanceVal.LogDebug($"Total users: {count}");
            return count;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get user count: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously gets the count of users
    /// </summary>
    /// <returns>Task containing total number of users</returns>
    public async Task<int> CountAsync()
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Getting user count asynchronously");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM `user`", connection);
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            
            LoggingServiceImpl.InstanceVal.LogDebug($"Total users: {count}");
            return count;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get user count: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets users by condition
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Collection of users matching the condition</returns>
    public IEnumerable<User> GetByCondition(string condition, object? parameters = null)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Getting users by condition: {condition}");

            var users = new List<User>();

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand($"SELECT id, username, password_hash FROM `user` WHERE {condition}", connection);

            if (parameters != null)
            {
                // Add parameters if provided
                var paramDict = parameters.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));
                
                foreach (var param in paramDict)
                {
                    cmd.Parameters.AddWithValue($"@{param.Key}", param.Value);
                }
            }

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                users.Add(user);
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"Found {users.Count} users matching condition");
            return users;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get users by condition: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously gets users by condition
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Task containing collection of users matching the condition</returns>
    public async Task<IEnumerable<User>> GetByConditionAsync(string condition, object? parameters = null)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Getting users by condition asynchronously: {condition}");

            var users = new List<User>();

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand($"SELECT id, username, password_hash FROM `user` WHERE {condition}", connection);

            if (parameters != null)
            {
                // Add parameters if provided
                var paramDict = parameters.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));
                
                foreach (var param in paramDict)
                {
                    cmd.Parameters.AddWithValue($"@{param.Key}", param.Value);
                }
            }

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                users.Add(user);
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"Found {users.Count} users matching condition");
            return users;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to get users by condition: {ex.Message}", ex);
            throw;
        }
    }

    // Advanced user management methods

    /// <summary>
    /// Finds a user by username
    /// </summary>
    /// <param name="username">Username to search for</param>
    /// <returns>User entity if found, null otherwise</returns>
    public User? FindByUsername(string username)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Finding user by username: {username}");

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "SELECT id, username, password_hash FROM `user` WHERE username = @username", 
                connection);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                
                LoggingServiceImpl.InstanceVal.LogDebug($"User found by username: {user}");
                return user;
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"User with username '{username}' not found");
            return null;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to find user by username '{username}': {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously finds a user by username
    /// </summary>
    /// <param name="username">Username to search for</param>
    /// <returns>Task containing user entity if found, null otherwise</returns>
    public async Task<User?> FindByUsernameAsync(string username)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Finding user by username asynchronously: {username}");

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT id, username, password_hash FROM `user` WHERE username = @username", 
                connection);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var user = new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash")
                );
                
                LoggingServiceImpl.InstanceVal.LogDebug($"User found by username: {user}");
                return user;
            }

            LoggingServiceImpl.InstanceVal.LogDebug($"User with username '{username}' not found");
            return null;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to find user by username '{username}': {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates user password with secure hashing
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newPassword">New plain text password</param>
    /// <returns>True if update successful, false otherwise</returns>
    public bool UpdatePassword(int userId, string newPassword)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Updating password for user ID {userId}");

            var hashedPassword = HashPassword(newPassword);

            using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "UPDATE `user` SET password_hash = @passwordHash WHERE id = @id", 
                connection);
            
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@passwordHash", hashedPassword);

            var rowsAffected = cmd.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"Password updated successfully for user ID {userId}");
                return true;
            }
            
            LoggingServiceImpl.InstanceVal.LogWarning($"User with ID {userId} not found for password update");
            return false;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to update password for user ID {userId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously updates user password with secure hashing
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newPassword">New plain text password</param>
    /// <returns>Task containing true if update successful, false otherwise</returns>
    public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogDebug($"Updating password for user ID {userId} asynchronously");

            var hashedPassword = HashPassword(newPassword);

            await using var connection = new MySqlConnection(DatabaseSetupUtility.DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "UPDATE `user` SET password_hash = @passwordHash WHERE id = @id", 
                connection);
            
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@passwordHash", hashedPassword);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"Password updated successfully for user ID {userId}");
                return true;
            }
            
            LoggingServiceImpl.InstanceVal.LogWarning($"User with ID {userId} not found for password update");
            return false;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to update password for user ID {userId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Hashes a password using PBKDF2 with salt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Base64 encoded hashed password</returns>
    private static string HashPassword(string password)
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
}
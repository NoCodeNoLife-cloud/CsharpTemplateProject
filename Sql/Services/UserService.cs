using MySqlConnector;
using Sql.Exceptions;
using Sql.Helpers;
using Sql.Interfaces;
using Sql.Models;

namespace Sql.Services;

/// <summary>
/// MySQL User service implementation
/// </summary>
public class UserService : ICrudService<User, int>, ITransactionService, IDisposable
{
    private readonly DatabaseConnectionManager _connectionManager;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the UserService class
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    public UserService(string connectionString)
    {
        _connectionManager = new DatabaseConnectionManager(connectionString);
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
    /// <exception cref="DuplicateKeyException">Thrown when username or email already exists</exception>
    public User Create(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        const string sql = @"
                INSERT INTO users (username, email, password_hash, is_active, created_at, updated_at)
                VALUES (@Username, @Email, @PasswordHash, @IsActive, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);

            var id = Convert.ToInt32(command.ExecuteScalar());
            user.Id = id;
            return user;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Username '{user.Username}' or email '{user.Email}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously creates a new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Task of created user</returns>
    public async Task<User> CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        const string sql = @"
                INSERT INTO users (username, email, password_hash, is_active, created_at, updated_at)
                VALUES (@Username, @Email, @PasswordHash, @IsActive, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
            user.Id = id;
            return user;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Username '{user.Username}' or email '{user.Email}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Found user, or null if not exists</returns>
    public User? GetById(int id)
    {
        const string sql = @"
                SELECT id, username, email, password_hash, is_active, created_at, updated_at
                FROM users
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapUserFromReader(reader) : null;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get user by ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Task of found user, or null if not exists</returns>
    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
                SELECT id, username, email, password_hash, is_active, created_at, updated_at
                FROM users
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return reader.Read() ? MapUserFromReader(reader) : null;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get user by ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>Collection of all users</returns>
    public IEnumerable<User> GetAll()
    {
        const string sql = @"
                SELECT id, username, email, password_hash, is_active, created_at, updated_at
                FROM users
                WHERE is_active = 1
                ORDER BY created_at DESC";

        var users = new List<User>();

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(MapUserFromReader(reader));
            }

            return users;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get all users: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets all users
    /// </summary>
    /// <returns>Task of all users collection</returns>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = @"
                SELECT id, username, email, password_hash, is_active, created_at, updated_at
                FROM users
                WHERE is_active = 1
                ORDER BY created_at DESC";

        var users = new List<User>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                users.Add(MapUserFromReader(reader));
            }

            return users;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get all users: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="user">User to update</param>
    /// <returns>Updated user</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null</exception>
    /// <exception cref="EntityNotFoundException">Thrown when user does not exist</exception>
    public User Update(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        const string sql = @"
                UPDATE users
                SET username = @Username,
                    email = @Email,
                    password_hash = @PasswordHash,
                    is_active = @IsActive,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", user.Id);

            var affectedRows = command.ExecuteNonQuery();
            if (affectedRows == 0)
            {
                throw new EntityNotFoundException(nameof(User), user.Id);
            }

            return user;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Username '{user.Username}' or email '{user.Email}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously updates a user
    /// </summary>
    /// <param name="user">User to update</param>
    /// <returns>Task of updated user</returns>
    public async Task<User> UpdateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        const string sql = @"
                UPDATE users
                SET username = @Username,
                    email = @Email,
                    password_hash = @PasswordHash,
                    is_active = @IsActive,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", user.Id);

            var affectedRows = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (affectedRows == 0)
            {
                throw new EntityNotFoundException(nameof(User), user.Id);
            }

            return user;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Username '{user.Username}' or email '{user.Email}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes a user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deletion successful, false otherwise</returns>
    public bool Delete(int id)
    {
        const string sql = @"
                UPDATE users
                SET is_active = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", id);

            var affectedRows = command.ExecuteNonQuery();
            return affectedRows > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes a user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Task of true if deletion successful, false otherwise</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
                UPDATE users
                SET is_active = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", id);

            var affectedRows = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return affectedRows > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates multiple users in batch
    /// </summary>
    /// <param name="users">Collection of users to create</param>
    /// <returns>Collection of created users</returns>
    public IEnumerable<User> CreateBatch(IEnumerable<User> users)
    {
        if (users == null)
            throw new ArgumentNullException(nameof(users));

        var userList = users.ToList();
        if (!userList.Any())
            return userList;

        const string sql = @"
                INSERT INTO users (username, email, password_hash, is_active, created_at, updated_at)
                VALUES (@Username, @Email, @PasswordHash, @IsActive, @CreatedAt, @UpdatedAt)";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var user in userList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);
                    command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);

                    command.ExecuteNonQuery();
                    user.Id = (int)command.LastInsertedId;
                }

                transaction.Commit();
                return userList;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more usernames or emails already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create users in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously creates multiple users in batch
    /// </summary>
    /// <param name="users">Collection of users to create</param>
    /// <returns>Task of created users collection</returns>
    public async Task<IEnumerable<User>> CreateBatchAsync(IEnumerable<User> users)
    {
        if (users == null)
            throw new ArgumentNullException(nameof(users));

        var userList = users.ToList();
        if (!userList.Any())
            return userList;

        const string sql = @"
                INSERT INTO users (username, email, password_hash, is_active, created_at, updated_at)
                VALUES (@Username, @Email, @PasswordHash, @IsActive, @CreatedAt, @UpdatedAt)";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                foreach (var user in userList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);
                    command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    user.Id = (int)command.LastInsertedId;
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return userList;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more usernames or emails already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create users in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates multiple users in batch
    /// </summary>
    /// <param name="users">Collection of users to update</param>
    /// <returns>Collection of updated users</returns>
    public IEnumerable<User> UpdateBatch(IEnumerable<User> users)
    {
        if (users == null)
            throw new ArgumentNullException(nameof(users));

        var userList = users.ToList();
        if (!userList.Any())
            return userList;

        const string sql = @"
                UPDATE users
                SET username = @Username,
                    email = @Email,
                    password_hash = @PasswordHash,
                    is_active = @IsActive,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var user in userList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", user.Id);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                return userList;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more usernames or emails already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update users in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously updates multiple users in batch
    /// </summary>
    /// <param name="users">Collection of users to update</param>
    /// <returns>Task of updated users collection</returns>
    public async Task<IEnumerable<User>> UpdateBatchAsync(IEnumerable<User> users)
    {
        if (users == null)
            throw new ArgumentNullException(nameof(users));

        var userList = users.ToList();
        if (!userList.Any())
            return userList;

        const string sql = @"
                UPDATE users
                SET username = @Username,
                    email = @Email,
                    password_hash = @PasswordHash,
                    is_active = @IsActive,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                foreach (var user in userList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", user.Id);

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return userList;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more usernames or emails already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update users in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes multiple users in batch
    /// </summary>
    /// <param name="ids">Collection of user IDs to delete</param>
    /// <returns>Number of successfully deleted items</returns>
    public int DeleteBatch(IEnumerable<int> ids)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids.ToList();
        if (!idList.Any())
            return 0;

        const string sql = @"
                UPDATE users
                SET is_active = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                int deletedCount = 0;
                foreach (var id in idList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", id);

                    deletedCount += command.ExecuteNonQuery();
                }

                transaction.Commit();
                return deletedCount;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete users in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes multiple users in batch
    /// </summary>
    /// <param name="ids">Collection of user IDs to delete</param>
    /// <returns>Task of number of successfully deleted items</returns>
    public async Task<int> DeleteBatchAsync(IEnumerable<int> ids)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids.ToList();
        if (!idList.Any())
            return 0;

        const string sql = @"
                UPDATE users
                SET is_active = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                int deletedCount = 0;
                foreach (var id in idList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", id);

                    deletedCount += await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return deletedCount;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete users in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if a user exists
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if exists, false otherwise</returns>
    public bool Exists(int id)
    {
        const string sql = "SELECT COUNT(1) FROM users WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to check if user exists: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously checks if a user exists
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Task of true if exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM users WHERE id = @Id AND is_active = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
            return count > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to check if user exists: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the count of users
    /// </summary>
    /// <returns>Total number of users</returns>
    public int Count()
    {
        const string sql = "SELECT COUNT(1) FROM users WHERE is_active = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            return Convert.ToInt32(command.ExecuteScalar());
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get user count: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets the count of users
    /// </summary>
    /// <returns>Task of total number of users</returns>
    public async Task<int> CountAsync()
    {
        const string sql = "SELECT COUNT(1) FROM users WHERE is_active = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get user count: {ex.Message}", ex);
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
        var sql = $@"
                SELECT id, username, email, password_hash, is_active, created_at, updated_at
                FROM users
                WHERE is_active = 1 AND ({condition})
                ORDER BY created_at DESC";

        var users = new List<User>();

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            if (parameters != null)
            {
                var paramDict = parameters.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));

                foreach (var param in paramDict)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(MapUserFromReader(reader));
            }

            return users;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get users by condition: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets users by condition
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Task of users collection matching the condition</returns>
    public async Task<IEnumerable<User>> GetByConditionAsync(string condition, object? parameters = null)
    {
        var sql = $@"
                SELECT id, username, email, password_hash, is_active, created_at, updated_at
                FROM users
                WHERE is_active = 1 AND ({condition})
                ORDER BY created_at DESC";

        var users = new List<User>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            if (parameters != null)
            {
                var paramDict = parameters.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));

                foreach (var param in paramDict)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                users.Add(MapUserFromReader(reader));
            }

            return users;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get users by condition: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps User object from DataReader
    /// </summary>
    /// <param name="reader">DataReader object</param>
    /// <returns>Mapped user object</returns>
    private static User MapUserFromReader(MySqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32("id"),
            Username = reader.GetString("username"),
            Email = reader.GetString("email"),
            PasswordHash = reader.GetString("password_hash"),
            IsActive = reader.GetBoolean("is_active"),
            CreatedAt = reader.GetDateTime("created_at"),
            UpdatedAt = reader.GetDateTime("updated_at")
        };
    }

    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <returns>Transaction object</returns>
    public object BeginTransaction()
    {
        var connection = _connectionManager.GetConnection();
        return connection.BeginTransaction();
    }

    /// <summary>
    /// Asynchronously begins a transaction
    /// </summary>
    /// <returns>Task of transaction object</returns>
    public async Task<object> BeginTransactionAsync()
    {
        var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
        return await connection.BeginTransactionAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Commits a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    public void CommitTransaction(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            mysqlTransaction.Commit();
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Asynchronously commits a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    /// <returns>Task</returns>
    public async Task CommitTransactionAsync(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            await mysqlTransaction.CommitAsync().ConfigureAwait(false);
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Rolls back a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    public void RollbackTransaction(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            mysqlTransaction.Rollback();
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Asynchronously rolls back a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    /// <returns>Task</returns>
    public async Task RollbackTransactionAsync(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            await mysqlTransaction.RollbackAsync().ConfigureAwait(false);
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    public void ExecuteInTransaction(Action action)
    {
        using var transaction = (MySqlTransaction)BeginTransaction();
        try
        {
            action();
            CommitTransaction(transaction);
        }
        catch
        {
            RollbackTransaction(transaction);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Task</returns>
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        var transaction = await BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            await action().ConfigureAwait(false);
            await CommitTransactionAsync(transaction).ConfigureAwait(false);
        }
        catch
        {
            await RollbackTransactionAsync(transaction).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Execution result</returns>
    public TResult ExecuteInTransaction<TResult>(Func<TResult> func)
    {
        using var transaction = (MySqlTransaction)BeginTransaction();
        try
        {
            var result = func();
            CommitTransaction(transaction);
            return result;
        }
        catch
        {
            RollbackTransaction(transaction);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Task of execution result</returns>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func)
    {
        var transaction = await BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            var result = await func().ConfigureAwait(false);
            await CommitTransactionAsync(transaction).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await RollbackTransactionAsync(transaction).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Performs cleanup operations
    /// </summary>
    /// <param name="disposing">Whether disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connectionManager?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Releases all resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer
    /// </summary>
    ~UserService()
    {
        Dispose(false);
    }
}
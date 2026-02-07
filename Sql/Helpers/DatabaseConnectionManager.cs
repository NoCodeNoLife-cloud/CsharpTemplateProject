using MySqlConnector;
using Sql.Exceptions;

namespace Sql.Helpers;

/// <summary>
/// Database connection manager
/// Manages database connections and provides connection utilities
/// </summary>
public sealed class DatabaseConnectionManager : IDisposable
{
    private readonly string _connectionString;
    private MySqlConnection? _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the DatabaseConnectionManager class
    /// Initializes a new instance of the DatabaseConnectionManager class
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <exception cref="ArgumentException">Thrown when connection string is null or empty</exception>
    public DatabaseConnectionManager(string connectionString)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString)
            ? throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString))
            : connectionString;
    }

    /// <summary>
    /// Gets the database connection
    /// Gets the database connection
    /// </summary>
    /// <returns>MySqlConnection instance</returns>
    /// <exception cref="ConnectionException">Thrown when connection fails</exception>
    public MySqlConnection GetConnection()
    {
        try
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new MySqlConnection(_connectionString);
                _connection.Open();
            }

            return _connection;
        }
        catch (MySqlException ex)
        {
            throw new ConnectionException($"Failed to establish database connection: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets the database connection
    /// Asynchronously gets the database connection
    /// </summary>
    /// <returns>Task of MySqlConnection instance</returns>
    /// <exception cref="ConnectionException">Thrown when connection fails</exception>
    public async Task<MySqlConnection> GetConnectionAsync()
    {
        try
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new MySqlConnection(_connectionString);
                await _connection.OpenAsync().ConfigureAwait(false);
            }

            return _connection;
        }
        catch (MySqlException ex)
        {
            throw new ConnectionException($"Failed to establish database connection: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if the database connection is available
    /// Checks if the database connection is available
    /// </summary>
    /// <returns>Returns true if connection is available, otherwise false</returns>
    public bool IsConnectionAvailable()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection.Ping();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Asynchronously checks if the database connection is available
    /// Asynchronously checks if the database connection is available
    /// </summary>
    /// <returns>Task that returns true if connection is available, otherwise false</returns>
    public async Task<bool> IsConnectionAvailableAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return await connection.PingAsync().ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Closes the database connection
    /// Closes the database connection
    /// </summary>
    public void CloseConnection()
    {
        try
        {
            if (_connection?.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }
        catch (MySqlException ex)
        {
            throw new ConnectionException($"Failed to close database connection: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously closes the database connection
    /// Asynchronously closes the database connection
    /// </summary>
    public async Task CloseConnectionAsync()
    {
        try
        {
            if (_connection?.State == System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync().ConfigureAwait(false);
            }
        }
        catch (MySqlException ex)
        {
            throw new ConnectionException($"Failed to close database connection: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Performs cleanup operations
    /// Performs cleanup operations
    /// </summary>
    /// <param name="disposing">Whether managed resources are being disposed</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    CloseConnection();
                    _connection?.Dispose();
                }
                catch
                {
                    // Ignore exceptions during cleanup
                    // Ignore exceptions during cleanup
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Releases all resources
    /// Releases all resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer
    /// Finalizer
    /// </summary>
    ~DatabaseConnectionManager()
    {
        Dispose(false);
    }
}
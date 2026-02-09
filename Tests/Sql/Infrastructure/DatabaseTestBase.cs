using MySqlConnector;
using Sql.Helpers;

namespace Tests.Sql.Infrastructure;

/// <summary>
/// Base class for database integration tests
/// Provides isolated test database for each test class
/// </summary>
public abstract class DatabaseTestBase : IAsyncLifetime
{
    private string? _testDatabaseName;
    private string? _testConnectionString;
    protected DatabaseConnectionManager? ConnectionManager { get; private set; }

    /// <summary>
    /// Gets the test database name (available after InitializeAsync)
    /// </summary>
    protected string TestDatabaseName => _testDatabaseName ??
                                         throw new InvalidOperationException("Test database not initialized");

    /// <summary>
    /// Gets the test connection string (available after InitializeAsync)
    /// </summary>
    protected string TestConnectionString => _testConnectionString ??
                                             throw new InvalidOperationException("Test database not initialized");

    /// <summary>
    /// Called before each test class runs
    /// Creates isolated test database
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        _testDatabaseName = TestDatabaseFactory.GenerateUniqueDatabaseName();
        _testConnectionString = await TestDatabaseFactory.CreateTestDatabaseAsync(_testDatabaseName);
        ConnectionManager = new DatabaseConnectionManager(_testConnectionString);
    }

    /// <summary>
    /// Called after each test class runs
    /// Cleans up test database
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        try
        {
            ConnectionManager?.Dispose();

            if (_testDatabaseName != null)
            {
                await TestDatabaseFactory.DropTestDatabaseAsync(_testDatabaseName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to cleanup test database: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes SQL command on test database
    /// </summary>
    /// <param name="sql">SQL command to execute</param>
    protected async Task ExecuteSqlCommandAsync(string sql)
    {
        await using var connection = new MySqlConnection(TestConnectionString);
        await connection.OpenAsync();
        using var cmd = new MySqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Executes SQL query and returns scalar result
    /// </summary>
    /// <param name="sql">SQL query to execute</param>
    /// <returns>Scalar result</returns>
    protected async Task<object?> ExecuteScalarAsync(string sql)
    {
        await using var connection = new MySqlConnection(TestConnectionString);
        await connection.OpenAsync();
        using var cmd = new MySqlCommand(sql, connection);
        return await cmd.ExecuteScalarAsync();
    }
}
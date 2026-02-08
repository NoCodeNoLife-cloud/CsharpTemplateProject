using MySqlConnector;

namespace Tests.Sql;

/// <summary>
/// Test database factory for creating isolated test databases
/// Each test class gets its own unique database to avoid conflicts
/// </summary>
public static class TestDatabaseFactory
{
    private static readonly Lock Lock = new();
    private static int _databaseCounter;

    /// <summary>
    /// Creates a unique test database name
    /// </summary>
    /// <returns>Unique database name</returns>
    public static string GenerateUniqueDatabaseName()
    {
        lock (Lock)
        {
            return $"test_db_{DateTime.Now:yyyyMMdd_HHmmss}_{Interlocked.Increment(ref _databaseCounter)}";
        }
    }

    /// <summary>
    /// Creates a test database with the given name
    /// </summary>
    /// <param name="databaseName">Database name to create</param>
    /// <returns>Connection string for the created database</returns>
    public static async Task<string> CreateTestDatabaseAsync(string databaseName)
    {
        try
        {
            // Connect to MySQL server without specifying database
            await using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            await connection.OpenAsync();

            // Create test database
            await using var cmd = new MySqlCommand($"CREATE DATABASE `{databaseName}`", connection);
            await cmd.ExecuteNonQueryAsync();

            return $"Server={DatabaseParam.AdminServer};Database={databaseName};Uid={DatabaseParam.AdminUid};Pwd={DatabaseParam.AdminPwd};";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create test database '{databaseName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Drops a test database
    /// </summary>
    /// <param name="databaseName">Database name to drop</param>
    public static async Task DropTestDatabaseAsync(string databaseName)
    {
        try
        {
            await using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand($"DROP DATABASE IF EXISTS `{databaseName}`", connection);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Log the error but don't throw in cleanup
            Console.WriteLine($"Warning: Failed to drop test database '{databaseName}': {ex.Message}");
        }
    }
}
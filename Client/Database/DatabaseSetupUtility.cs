using Client.Database.Config;
using CustomSerilogImpl.InstanceVal.Service.Services;
using MySqlConnector;

namespace Client.Database;

/// <summary>
/// Database setup utility for initializing and verifying MySQL database environment
/// </summary>
public static class DatabaseSetupUtility
{
    private const string DemoDatabaseName = "demo";

    /// <summary>
    /// Gets the connection string for the demo database
    /// </summary>
    public static string DemoConnectionString =>
        $"Server={DatabaseParam.AdminServer};Database={DemoDatabaseName};Uid={DatabaseParam.AdminUid};Pwd={DatabaseParam.AdminPwd};";

    /// <summary>
    /// Checks if MySQL can connect and sets up the demo database environment
    /// </summary>
    /// <returns>True if setup successful, false otherwise</returns>
    public static async Task<bool> SetupDemoDatabaseAsync()
    {
        try
        {
            LoggingFactory.Instance.LogDebug("Starting database setup process...");

            // Step 1: Check if MySQL server is accessible
            LoggingFactory.Instance.LogDebug("Step 1: Checking MySQL server connectivity...");
            if (!await CheckMySqlServerConnectivityAsync())
            {
                LoggingFactory.Instance.LogError("Cannot connect to MySQL server. Please ensure MySQL is running and credentials are correct.");
                return false;
            }

            LoggingFactory.Instance.LogDebug("MySQL server connectivity check passed.");

            // Step 2: Check if demo database exists, create if not
            LoggingFactory.Instance.LogDebug("Step 2: Checking/creating demo database...");
            if (!await EnsureDemoDatabaseExistsAsync())
            {
                LoggingFactory.Instance.LogError("Failed to create/access demo database.");
                return false;
            }

            LoggingFactory.Instance.LogDebug("Demo database is ready.");

            // Step 3: Check if user table exists, create if not
            LoggingFactory.Instance.LogDebug("Step 3: Checking/creating user table...");
            if (!await EnsureUserTableExistsAsync())
            {
                LoggingFactory.Instance.LogError("Failed to create/access user table.");
                return false;
            }

            LoggingFactory.Instance.LogDebug("User table is ready.");

            LoggingFactory.Instance.LogInformation("Database setup completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Database setup failed: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Checks if MySQL server is accessible with admin credentials
    /// </summary>
    private static async Task<bool> CheckMySqlServerConnectivityAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            await connection.OpenAsync();
            return await connection.PingAsync();
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogDebug($"MySQL connectivity check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ensures the demo database exists, creates it if necessary
    /// </summary>
    private static async Task<bool> EnsureDemoDatabaseExistsAsync()
    {
        try
        {
            // First check if database already exists
            if (await CheckDatabaseExistsAsync(DemoDatabaseName))
            {
                LoggingFactory.Instance.LogDebug($"Demo database '{DemoDatabaseName}' already exists.");
                return true;
            }

            // Create the database
            LoggingFactory.Instance.LogDebug($"Creating demo database '{DemoDatabaseName}'...");
            await using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand($"CREATE DATABASE `{DemoDatabaseName}`", connection);
            await cmd.ExecuteNonQueryAsync();

            LoggingFactory.Instance.LogDebug($"Successfully created demo database '{DemoDatabaseName}'.");
            return true;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Failed to ensure demo database exists: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if a specific database exists
    /// </summary>
    private static async Task<bool> CheckDatabaseExistsAsync(string databaseName)
    {
        try
        {
            await using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @dbName", connection);
            cmd.Parameters.AddWithValue("@dbName", databaseName);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogDebug($"Database existence check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ensures the user table exists in the demo database, creates it if necessary
    /// Handles migration from old table structure if needed
    /// </summary>
    private static async Task<bool> EnsureUserTableExistsAsync()
    {
        try
        {
            // Check if table already exists
            if (await CheckTableExistsAsync(DemoDatabaseName, "user"))
            {
                LoggingFactory.Instance.LogDebug("User table already exists");
                return true;
            }

            // Create the user table
            LoggingFactory.Instance.LogDebug("Creating user table...");
            const string createTableSql =
                """
                CREATE TABLE `user` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `username` VARCHAR(50) NOT NULL,
                    `password_hash` VARCHAR(255) NOT NULL,
                    UNIQUE KEY `unique_username` (`username`),
                    INDEX `idx_username` (`username`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """;

            await using var connection = new MySqlConnection(DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(createTableSql, connection);
            await cmd.ExecuteNonQueryAsync();

            LoggingFactory.Instance.LogDebug("Successfully created user table with id, username, and password_hash fields.");
            return true;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Failed to ensure user table exists: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if a specific table exists in a database
    /// </summary>
    private static async Task<bool> CheckTableExistsAsync(string databaseName, string tableName)
    {
        try
        {
            await using var connection = new MySqlConnection(DemoConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = @dbName AND TABLE_NAME = @tableName",
                connection);
            cmd.Parameters.AddWithValue("@dbName", databaseName);
            cmd.Parameters.AddWithValue("@tableName", tableName);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogDebug($"Table existence check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tests the database connection with the demo database
    /// </summary>
    public static async Task<bool> TestDemoDatabaseConnectionAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(DemoConnectionString);
            await connection.OpenAsync();
            var result = await connection.PingAsync();
            LoggingFactory.Instance.LogDebug($"Demo database connection test: {(result ? "SUCCESS" : "FAILED")}");
            return result;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Demo database connection test failed: {ex.Message}");
            return false;
        }
    }
}
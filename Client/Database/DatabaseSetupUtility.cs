using Client.Config;
using LoggingService.Services;
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
            LoggingServiceImpl.InstanceVal.LogDebug("Starting database setup process...");

            // Step 1: Check if MySQL server is accessible
            LoggingServiceImpl.InstanceVal.LogDebug("Step 1: Checking MySQL server connectivity...");
            if (!await CheckMySqlServerConnectivityAsync())
            {
                LoggingServiceImpl.InstanceVal.LogError("Cannot connect to MySQL server. Please ensure MySQL is running and credentials are correct.");
                return false;
            }

            LoggingServiceImpl.InstanceVal.LogDebug("MySQL server connectivity check passed.");

            // Step 2: Check if demo database exists, create if not
            LoggingServiceImpl.InstanceVal.LogDebug("Step 2: Checking/creating demo database...");
            if (!await EnsureDemoDatabaseExistsAsync())
            {
                LoggingServiceImpl.InstanceVal.LogError("Failed to create/access demo database.");
                return false;
            }

            LoggingServiceImpl.InstanceVal.LogDebug("Demo database is ready.");

            // Step 3: Check if user table exists, create if not
            LoggingServiceImpl.InstanceVal.LogDebug("Step 3: Checking/creating user table...");
            if (!await EnsureUserTableExistsAsync())
            {
                LoggingServiceImpl.InstanceVal.LogError("Failed to create/access user table.");
                return false;
            }

            LoggingServiceImpl.InstanceVal.LogDebug("User table is ready.");

            LoggingServiceImpl.InstanceVal.LogInformation("Database setup completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database setup failed: {ex.Message}", ex);
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
            LoggingServiceImpl.InstanceVal.LogDebug($"MySQL connectivity check failed: {ex.Message}");
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
                LoggingServiceImpl.InstanceVal.LogDebug($"Demo database '{DemoDatabaseName}' already exists.");
                return true;
            }

            // Create the database
            LoggingServiceImpl.InstanceVal.LogDebug($"Creating demo database '{DemoDatabaseName}'...");
            await using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            await connection.OpenAsync();

            await using var cmd = new MySqlCommand($"CREATE DATABASE `{DemoDatabaseName}`", connection);
            await cmd.ExecuteNonQueryAsync();

            LoggingServiceImpl.InstanceVal.LogDebug($"Successfully created demo database '{DemoDatabaseName}'.");
            return true;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to ensure demo database exists: {ex.Message}");
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
            LoggingServiceImpl.InstanceVal.LogDebug($"Database existence check failed: {ex.Message}");
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
                LoggingServiceImpl.InstanceVal.LogDebug("User table already exists");
                return true;
            }

            // Create the user table
            LoggingServiceImpl.InstanceVal.LogDebug("Creating user table...");
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

            LoggingServiceImpl.InstanceVal.LogDebug("Successfully created user table with id, username, and password_hash fields.");
            return true;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Failed to ensure user table exists: {ex.Message}");
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
            LoggingServiceImpl.InstanceVal.LogDebug($"Table existence check failed: {ex.Message}");
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
            LoggingServiceImpl.InstanceVal.LogDebug($"Demo database connection test: {(result ? "SUCCESS" : "FAILED")}");
            return result;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Demo database connection test failed: {ex.Message}");
            return false;
        }
    }
}
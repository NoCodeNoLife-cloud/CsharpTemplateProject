using CommonFramework.Aop.Attributes;
using LoggingService.Services;
using LoggingService.Enums;
using ClientApplication.Config;
using ClientApplication.Database;
using ClientApplication.Database.Examples;

namespace ClientApplication.App;

/// <summary>
/// Contains the main entry point of the application
/// </summary>
internal static class Program
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    [Log(LogLevel = LogLevel.Debug, LogMethodEntry = false)]
    private static async Task Main()
    {
        // Print Banner
        Banner.Banner.PrintBanner();

        // Print project root directory using Framework logging service
        LoggingServiceImpl.InstanceVal.LogDebug($"Project Root Directory: {EnvironmentPath.ProjectRootDirectory}");

        // Setup and verify database environment
        LoggingServiceImpl.InstanceVal.LogDebug("Starting database environment setup...");
        var databaseSetupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
        
        if (databaseSetupSuccess)
        {
            LoggingServiceImpl.InstanceVal.LogInformation("Database environment is ready for use.");
            
            // Test the connection
            var connectionTest = await DatabaseSetupUtility.TestDemoDatabaseConnectionAsync();
            if (connectionTest)
            {
                LoggingServiceImpl.InstanceVal.LogInformation("Database connection test successful.");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning("Database connection test failed, but setup completed.");
            }
        }
        else
        {
            LoggingServiceImpl.InstanceVal.LogError("Failed to setup database environment. Application may not function correctly.");
        }

        // Demonstrate database usage with example operations
        if (databaseSetupSuccess)
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Demonstrating database operations...");
            await DemonstrateDatabaseOperationsAsync();
        }

        // Application logic can be added here
    }

    /// <summary>
    /// Demonstrates basic database operations
    /// </summary>
    private static async Task DemonstrateDatabaseOperationsAsync()
    {
        try
        {
            // Add some sample users (will skip if they already exist)
            LoggingServiceImpl.InstanceVal.LogDebug("Adding sample users (skipping duplicates)...");
            var adminAdded = await UserManagementExample.AddUserAsync("admin", "admin123");
            var user1Added = await UserManagementExample.AddUserAsync("user1", "password1");
            var user2Added = await UserManagementExample.AddUserAsync("user2", "password2");
            
            LoggingServiceImpl.InstanceVal.LogInformation($"Sample users processing complete: Admin {(adminAdded ? "processed" : "skipped")}, User1 {(user1Added ? "processed" : "skipped")}, User2 {(user2Added ? "processed" : "skipped")}");

            // Retrieve a user
            LoggingServiceImpl.InstanceVal.LogDebug("Retrieving user 'admin'...");
            var (found, userId, username) = await UserManagementExample.GetUserAsync("admin");
            if (found)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"Retrieved user: ID={userId}, Username={username}");
            }

            // List all users
            LoggingServiceImpl.InstanceVal.LogDebug("Listing all users...");
            var users = await UserManagementExample.ListAllUsersAsync();
            foreach (var (id, user) in users)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User #{id}: {user}");
            }

            // Demonstrate safe user insertion (handles duplicates)
            LoggingServiceImpl.InstanceVal.LogDebug("Demonstrating safe user insertion with duplicate handling...");
            await UserManagementExample.DemonstrateSafeUserInsertionAsync();

            // Run duplicate prevention test
            LoggingServiceImpl.InstanceVal.LogDebug("Running duplicate prevention test...");
            var testResult = await UserInsertionTest.TestDuplicatePreventionAsync();
            LoggingServiceImpl.InstanceVal.LogInformation($"Duplicate prevention test {(testResult ? "PASSED" : "FAILED")}");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error in database demonstration: {ex.Message}");
        }
    }
}
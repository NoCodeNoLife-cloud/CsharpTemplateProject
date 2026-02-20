using Client.Config;
using Client.Database;
using CommonFramework.Aop.Attributes;
using CommonFramework.Banner;
using LoggingService.Enums;
using LoggingService.Services;

namespace Client.App;

/// <summary>
/// Application initialization handler
/// </summary>
internal static class ApplicationInitializer
{
    /// <summary>
    /// Initialize application components with progress indication
    /// </summary>
    [Log(LogLevel = LogLevel.Debug, LogMethodEntry = false)]
    public static async Task InitializeAsync()
    {
        LoggingServiceImpl.InstanceVal.LogDebug($"Project Root Directory: {EnvironmentPath.GetProjectRootDirectory()}");
        await Task.Delay(500); // Simulate work
        LoggingServiceImpl.InstanceVal.LogInformation("Environment configured");

        // Step 2: Database setup
        LoggingServiceImpl.InstanceVal.LogDebug("Starting database environment setup...");
        var databaseSetupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
        await Task.Delay(800); // Simulate work

        if (databaseSetupSuccess)
        {
            LoggingServiceImpl.InstanceVal.LogInformation("Database environment ready");
        }
        else
        {
            LoggingServiceImpl.InstanceVal.LogError("Database setup failed");
            throw new InvalidOperationException("Failed to setup database environment");
        }

        // Step 3: Connection test
        var connectionTest = await DatabaseSetupUtility.TestDemoDatabaseConnectionAsync();
        await Task.Delay(300); // Simulate work

        if (connectionTest)
        {
            LoggingServiceImpl.InstanceVal.LogInformation("Database connection established and test successful");
        }
        else
        {
            LoggingServiceImpl.InstanceVal.LogWarning("Database connection test failed, but setup completed");
        }

        // Step 4: Final initialization
        await Task.Delay(200); // Simulate work
        LoggingServiceImpl.InstanceVal.LogInformation("System initialization complete! Application is ready for use!");
        await Task.Delay(1000);
    }
}
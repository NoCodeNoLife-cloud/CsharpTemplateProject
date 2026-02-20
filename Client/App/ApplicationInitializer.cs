using Client.Config;
using Client.Database;
using CommonFramework.Aop.Attributes;
using CommonFramework.Banner;
using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Services;

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
        LoggingFactory.Instance.LogDebug($"Project Root Directory: {EnvironmentPath.GetProjectRootDirectory()}");
        await Task.Delay(500); // Simulate work
        LoggingFactory.Instance.LogInformation("Environment configured");

        // Step 2: Database setup
        LoggingFactory.Instance.LogDebug("Starting database environment setup...");
        var databaseSetupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
        await Task.Delay(800); // Simulate work

        if (databaseSetupSuccess)
        {
            LoggingFactory.Instance.LogInformation("Database environment ready");
        }
        else
        {
            LoggingFactory.Instance.LogError("Database setup failed");
            throw new InvalidOperationException("Failed to setup database environment");
        }

        // Step 3: Connection test
        var connectionTest = await DatabaseSetupUtility.TestDemoDatabaseConnectionAsync();
        await Task.Delay(300); // Simulate work

        if (connectionTest)
        {
            LoggingFactory.Instance.LogInformation("Database connection established and test successful");
        }
        else
        {
            LoggingFactory.Instance.LogWarning("Database connection test failed, but setup completed");
        }

        // Step 4: Final initialization
        await Task.Delay(200); // Simulate work
        LoggingFactory.Instance.LogInformation("System initialization complete! Application is ready for use!");
        await Task.Delay(1000);
    }
}
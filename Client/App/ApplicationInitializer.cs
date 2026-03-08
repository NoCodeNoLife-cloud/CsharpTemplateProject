using Client.App.Services;
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

        // Step 2: Database setup via Server API
        LoggingFactory.Instance.LogDebug("Requesting database setup from Server...");
        var (setupSuccess, setupMessage) = await ServerAuthService.SetupDatabaseAsync();
        await Task.Delay(800); // Simulate work

        if (setupSuccess)
        {
            LoggingFactory.Instance.LogInformation($"Database environment ready: {setupMessage}");
        }
        else
        {
            LoggingFactory.Instance.LogError($"Database setup failed: {setupMessage}");
            throw new InvalidOperationException($"Failed to setup database environment: {setupMessage}");
        }

        // Step 3: Connection test via Server API
        LoggingFactory.Instance.LogDebug("Requesting database connection test from Server...");
        var (testSuccess, isConnected, databaseName, testedAt, testMessage) = await ServerAuthService.TestDatabaseConnectionAsync();
        await Task.Delay(300); // Simulate work

        if (testSuccess && isConnected == true)
        {
            LoggingFactory.Instance.LogInformation($"Database connection established and test successful (Database: {databaseName}, Tested at: {testedAt?.ToLocalTime()})");
        }
        else
        {
            LoggingFactory.Instance.LogWarning($"Database connection test failed: {testMessage}");
        }

        // Step 4: Final initialization
        await Task.Delay(200); // Simulate work
        LoggingFactory.Instance.LogInformation("System initialization complete! Application is ready for use!");
        await Task.Delay(1000);
    }
}
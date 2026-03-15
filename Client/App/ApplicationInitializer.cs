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
    private const int EnvironmentConfigDelayMs = 500;
    private const int DatabaseSetupDelayMs = 800;
    private const int ConnectionTestDelayMs = 300;
    private const int FinalInitializationDelayMs = 200;
    private const int CompletionDisplayDelayMs = 1000;

    /// <summary>
    /// Initialize application components with progress indication
    /// </summary>
    [Log(LogLevel = LogLevel.Debug, LogMethodEntry = false)]
    public static async Task InitializeAsync()
    {
        LoggingFactory.Instance.LogDebug($"Project Root Directory: {EnvironmentPath.GetProjectRootDirectory()}");
        await Task.Delay(EnvironmentConfigDelayMs);
        LoggingFactory.Instance.LogInformation("Environment configured");

        // Step 2: Database setup via Server API
        var (setupSuccess, setupMessage) = await ServerAuthService.SetupDatabaseAsync();
        await Task.Delay(DatabaseSetupDelayMs);

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
        var (testSuccess, isConnected, databaseName, testedAt, testMessage) = await ServerAuthService.TestDatabaseConnectionAsync();
        await Task.Delay(ConnectionTestDelayMs);

        if (testSuccess && isConnected == true)
        {
            LoggingFactory.Instance.LogInformation($"Database connection established and test successful (Database: {databaseName}, Tested at: {testedAt?.ToLocalTime()})");
        }
        else
        {
            LoggingFactory.Instance.LogWarning($"Database connection test failed: {testMessage}");
        }

        // Step 4: Final initialization
        await Task.Delay(FinalInitializationDelayMs);
        LoggingFactory.Instance.LogInformation("System initialization complete! Application is ready for use!");
        await Task.Delay(CompletionDisplayDelayMs);
    }
}
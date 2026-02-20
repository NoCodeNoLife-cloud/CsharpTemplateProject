using Client.App.Manu;
using CommonFramework.Aop.Attributes;
using CommonFramework.Banner;
using LoggingService.Enums;
using LoggingService.Services;

namespace Client.App;

/// <summary>
/// Contains the main entry point of the application
/// </summary>
internal static class Program
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    [Log(LogLevel = LogLevel.Debug, LogMethodEntry = false)]
    [Obsolete("Obsolete")]
    private static async Task Main()
    {
        try
        {
            // Print enhanced Banner
            BannerManager.PrintBanner();

            // Enhanced startup sequence
            await ApplicationInitializer.InitializeAsync();

            // Interactive user management
            await InteractiveUserManagementAsync();
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Application startup failed: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to exit...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Interactive user management - delegates to menu navigator
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task InteractiveUserManagementAsync()
    {
        await MenuNavigator.NavigateMainMenuAsync();
    }
}
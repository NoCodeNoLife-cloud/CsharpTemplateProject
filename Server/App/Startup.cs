using CommonFramework.Aop.Attributes;
using CommonFramework.Banner;
using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Server.App;

/// <summary>
/// Contains the main entry point of the application
/// </summary>
internal static class Startup
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
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Application startup failed: {ex.Message}", ex);
            LoggingFactory.Instance.LogDebug("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
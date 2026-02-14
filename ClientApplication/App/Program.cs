using CommonFramework.Aop.Attributes;
using LoggingService.Services;
using LoggingService.Enums;
using ClientApplication.Config;

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
    private static void Main()
    {
        // Print Banner
        PrintBanner();

        // Print project root directory using Framework logging service
        LoggingServiceImpl.InstanceVal.LogDebug($"Project Root Directory: {EnvironmentPath.ProjectRootDirectory}");

        // Application logic can be added here
    }

    /// <summary>
    /// Print application banner
    /// </summary>
    private static void PrintBanner()
    {
        try
        {
            var bannerPath = EnvironmentPath.GetBannerPath();
            if (EnvironmentPath.IsBannerFileExists())
            {
                var bannerContent = File.ReadAllText(bannerPath);
                Console.WriteLine(bannerContent);
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning("Banner file not found");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Access denied to banner file: {ex.Message}", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Directory not found: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"IO error reading banner file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error reading banner file: {ex.Message}", ex);
        }
    }
}
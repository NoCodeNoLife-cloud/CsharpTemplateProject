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
    [Log(LogLevel = LogLevel.Debug)]
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
            const string bannerFileName = "Banner.txt";
            var bannerPath = EnvironmentPath.GetBannerPath();
            if (EnvironmentPath.IsBannerFileExists())
            {
                var bannerContent = File.ReadAllText(bannerPath);
                Console.WriteLine(bannerContent);
            }
            else
            {
                Console.WriteLine("Banner file not found");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to banner file: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Directory not found: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"IO error reading banner file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error reading banner file: {ex.Message}");
        }
    }
}
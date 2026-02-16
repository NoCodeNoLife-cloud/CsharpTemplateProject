using LoggingService.Services;
using ClientApplication.Config;

namespace ClientApplication.App.Banner;

/// <summary>
/// Banner management class for handling application banner display
/// </summary>
public static class Banner
{
    /// <summary>
    /// Banner file relative path
    /// </summary>
    public const string BannerPath = "App/Banner/Banner.txt";

    /// <summary>
    /// Print application banner to console with enhanced visual effects
    /// </summary>
    public static void PrintBanner()
    {
        try
        {
            ConsoleHelper.ClearScreenWithHeader("Application Startup");
            
            if (IsBannerFileExists())
            {
                var bannerPath = GetBannerPath();
                var bannerContent = File.ReadAllText(bannerPath);
                
                // Print banner with color effect
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(bannerContent);
                Console.ResetColor();
                
                // Add application info
                ConsoleHelper.PrintSeparator(60, '=');
                ConsoleHelper.PrintHighlight("C# Template Project v1.0");
                ConsoleHelper.PrintInfo($"Project Root: {EnvironmentPath.ProjectRootDirectory}");
                ConsoleHelper.PrintSeparator(60, '=');
                
                // Simulate startup animation
                ConsoleHelper.PrintInfo("Initializing application components...");
                for (int i = 1; i <= 3; i++)
                {
                    Console.Write(".");
                    Thread.Sleep(500);
                }
                Console.WriteLine();
                ConsoleHelper.PrintSuccess("Application initialized successfully!");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning("Banner file not found");
                ConsoleHelper.PrintWarning("Using fallback banner display");
                PrintSimpleBanner();
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Access denied to banner file: {ex.Message}", ex);
            ConsoleHelper.PrintError($"Access denied: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        { 
            LoggingServiceImpl.InstanceVal.LogError($"Directory not found: {ex.Message}", ex);
            ConsoleHelper.PrintError($"Directory not found: {ex.Message}");
        }
        catch (IOException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"IO error reading banner file: {ex.Message}", ex);
            ConsoleHelper.PrintError($"IO Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error reading banner file: {ex.Message}", ex);
            ConsoleHelper.PrintError($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Print a simple banner when file is not available
    /// </summary>
    private static void PrintSimpleBanner()
    {
        ConsoleHelper.PrintHeader("C# TEMPLATE PROJECT", ConsoleHelper.MessageType.Header);
        ConsoleHelper.PrintHighlight("Welcome to the C# Template Application");
        ConsoleHelper.PrintInfo("Ready for development and testing");
        ConsoleHelper.PrintSeparator();
    }

    /// <summary>
    /// Get complete Banner file path
    /// </summary>
    /// <returns>Full path of Banner file</returns>
    public static string GetBannerPath()
    {
        return Path.Combine(EnvironmentPath.ProjectRootDirectory ?? throw new InvalidOperationException(), BannerPath);
    }

    /// <summary>
    /// Check if Banner file exists
    /// </summary>
    /// <returns>Returns true if Banner file exists, otherwise false</returns>
    public static bool IsBannerFileExists()
    {
        return File.Exists(GetBannerPath());
    }
}
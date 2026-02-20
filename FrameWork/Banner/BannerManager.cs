using LoggingService.Services;

namespace CommonFramework.Banner;

/// <summary>
/// Banner management class for handling application banner display
/// </summary>
public static class BannerManager
{
    /// <summary>
    /// Banner file relative path
    /// </summary>
    public const string BannerPath = "Resources/Banner.txt";

    /// <summary>
    /// Print application banner to console with enhanced visual effects
    /// </summary>
    public static void PrintBanner()
    {
        try
        {
            var bannerPath = GetBannerPath();
            if (File.Exists(bannerPath))
            {
                var bannerContent = File.ReadAllText(bannerPath);
                Console.WriteLine(bannerContent);
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogError("Banner file not found");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Access denied to banner file: {ex.Message}", ex);
            Console.WriteLine($"Access denied: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Directory not found: {ex.Message}", ex);
            Console.WriteLine($"Directory not found: {ex.Message}");
        }
        catch (IOException ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"IO error reading banner file: {ex.Message}", ex);
            Console.WriteLine($"IO Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error reading banner file: {ex.Message}", ex);
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get complete Banner file path
    /// </summary>
    /// <returns>Full path of Banner file</returns>
    public static string GetBannerPath()
    {
        var projectPath = EnvironmentPath.GetProjectRootDirectory();

        // Validate that ProjectRootDirectory is resolved
        if (string.IsNullOrEmpty(projectPath))
        {
            throw new InvalidOperationException("Unable to resolve project root directory");
        }

        var bannerPath = Path.Combine(projectPath, BannerPath);
        LoggingServiceImpl.InstanceVal.LogDebug($"Resolved banner path: {bannerPath}");
        return bannerPath;
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
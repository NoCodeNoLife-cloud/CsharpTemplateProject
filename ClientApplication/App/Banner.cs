using LoggingService.Services;
using ClientApplication.Config;

namespace ClientApplication.App;

/// <summary>
/// Banner management class for handling application banner display
/// </summary>
public static class Banner
{
    /// <summary>
    /// Banner file relative path
    /// </summary>
    public const string BannerPath = "Resources/Banner.txt";

    /// <summary>
    /// Print application banner to console
    /// </summary>
    public static void PrintBanner()
    {
        try
        {
            var bannerPath = GetBannerPath();
            if (IsBannerFileExists())
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
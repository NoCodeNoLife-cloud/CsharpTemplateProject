using System.IO;
using System.Reflection;

namespace ClientApplication.Config;

/// <summary>
/// Static environment path management class for managing various path information of the application
/// Automatically initializes project root directory in static constructor
/// </summary>
public static class EnvironmentPath
{
    /// <summary>
    /// Project root directory path
    /// </summary>
    public static string? ProjectRootDirectory { get; private set; }

    public static string BannerPath => "Resources/Banner.txt";

    /// <summary>
    /// Static constructor, automatically executed when the class is first accessed
    /// </summary>
    static EnvironmentPath()
    {
        InitializePaths();
    }

    /// <summary>
    /// Initialize all path variables
    /// </summary>
    private static void InitializePaths()
    {
        try
        {
            // Method 1: Using Assembly Location (most common)
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var projectRoot1 = Path.GetDirectoryName(assemblyLocation);

            // Method 2: Using AppDomain Base Directory
            var appDomainBase = AppDomain.CurrentDomain.BaseDirectory;

            // Method 3: Search upward from current directory for .csproj file
            var currentDir = Directory.GetCurrentDirectory();
            var projectRoot3 = FindProjectRoot(currentDir, "ClientApplication.csproj");

            // Method 4: Using Environment.CurrentDirectory and search upward
            var envCurrentDir = Environment.CurrentDirectory;
            var projectRoot4 = FindProjectRoot(envCurrentDir, "ClientApplication.csproj");

            // Return the best project root directory (prioritize CSProj search results)
            ProjectRootDirectory = projectRoot3 ?? projectRoot4 ?? projectRoot1 ?? appDomainBase;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing environment paths: {ex.Message}");
        }
    }

    /// <summary>
    /// Recursively search upward for directory containing specified project file
    /// </summary>
    /// <param name="startDirectory">Starting directory</param>
    /// <param name="projectFileName">Project file name</param>
    /// <returns>Project root directory path, or null if not found</returns>
    private static string? FindProjectRoot(string startDirectory, string projectFileName)
    {
        var currentDir = new DirectoryInfo(startDirectory);

        while (currentDir != null)
        {
            var projectFile = Path.Combine(currentDir.FullName, projectFileName);
            if (File.Exists(projectFile))
            {
                return currentDir.FullName;
            }

            currentDir = currentDir.Parent;
        }

        return null;
    }

    /// <summary>
    /// Get Banner file path
    /// </summary>
    /// <returns>Complete path of Banner file</returns>
    public static string GetBannerPath()
    {
        return Path.Combine(ProjectRootDirectory ?? throw new InvalidOperationException(), BannerPath);
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
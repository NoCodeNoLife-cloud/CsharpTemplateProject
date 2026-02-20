using System.Reflection;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace CommonFramework.Banner;

/// <summary>
/// Static environment path management class for managing various path information of the application
/// Automatically initializes project root directory in static constructor
/// </summary>
public static class EnvironmentPath
{
    /// <summary>
    /// Initialize all path variables
    /// </summary>
    public static string? GetProjectRootDirectory()
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

            return projectRoot3 ?? projectRoot4 ?? projectRoot1 ?? appDomainBase;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error initializing environment paths: {ex.Message}");
            return null;
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
}
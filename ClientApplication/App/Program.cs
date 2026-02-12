using CommonFramework.Aop.Attributes;
using LoggingService.Services;

namespace ClientApplication.App;

/// <summary>
/// Contains the main entry point of the application
/// </summary>
internal static class Program
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    /// <param name="args">Command line arguments passed to the application</param>
    private static void Main(string[] args)
    {
        LoggingServiceImpl.InstanceVal.LogDebug("Starting main application process...");
        Thread.Sleep(1000);
        LoggingServiceImpl.InstanceVal.LogDebug("Main application process executed successfully.");
    }
}
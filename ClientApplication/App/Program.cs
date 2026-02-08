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
        LoggingServiceImpl.InstanceVal.LogDebug("INFO Testing completed. Check log outputs above.");
        
        Console.WriteLine("ClientApplication/App/Program.cs:1");
    }
}
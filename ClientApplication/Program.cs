using LoggingService.Services;

namespace ClientApplication;

internal static class Program
{
    private static void Main(string[] args)
    {
        var loggingService = LoggingServiceImpl.Instance;

        loggingService.LogInformation("Application started");
        loggingService.LogWarning("This is a warning message");
        loggingService.LogError("An error occurred", new Exception("Test exception"));
        loggingService.LogDebug("This is a debug message");
        loggingService.LogCritical("This is a critical message");

        Console.WriteLine("INFO Testing completed. Check log outputs above.");
    }
}
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
    [Log]
    private static void Main(string[] args)
    {
        Thread.Sleep(1000);
    }
}
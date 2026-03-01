using CommonFramework.Aop.Attributes;
using CommonFramework.Banner;
using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Server.App;

/// <summary>
/// Contains the main entry point of the application
/// </summary>
internal static class Startup
{
    /// <summary>
    /// WebHost instance stored as a class variable
    /// </summary>
    private static IWebHost? _host;

    /// <summary>
    /// Configures and builds the WebHost instance
    /// </summary>
    /// <returns>Configured IWebHost instance</returns>
    private static IWebHost ConfigureWebHost()
    {
        return new WebHostBuilder()
            .UseKestrel()
            .UseUrls("http://localhost:5000")
            .Configure(app =>
            {
                // Default handler for other paths
                app.Run(async context =>
                {
                    LoggingFactory.Instance.LogInformation($"Received request: {context.Request.Method} {context.Request.Path}");
                    await context.Response.WriteAsync("Welcome to the server! Use /test endpoint for echo functionality.");
                });
            })
            .Build();
    }

    /// <summary>
    /// Entry point of the application
    /// </summary>
    [Log(LogLevel = LogLevel.Debug, LogMethodEntry = false)]
    [Obsolete("Obsolete")]
    private static async Task Main(string[] args)
    {
        try
        {
            // Print enhanced Banner
            BannerManager.PrintBanner();

            LoggingFactory.Instance.LogInformation("Starting HTTP Echo Server...");

            // Configure and initialize the WebHost
            _host = ConfigureWebHost();

            LoggingFactory.Instance.LogInformation("Server is running on http://localhost:5000");
            await Console.Out.WriteLineAsync("Press Ctrl+C to stop the server");

            await _host.RunAsync();
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Application startup failed: {ex.Message}", ex);
            LoggingFactory.Instance.LogDebug("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
using CommonFramework.Aop.Attributes;
using CommonFramework.Banner;
using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Server.Database;
using Server.Database.UserAuthentication;

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
                // Define endpoint configurations
                var endpoints = new List<EndpointConfig>
                {
                    // User Authentication API endpoints
                    new() { Path = "/api/auth/login", Method = "POST", Handler = UserAuthApiService.HandleLoginAsync, ContentType = "application/json" },
                    new() { Path = "/api/auth/register", Method = "POST", Handler = UserAuthApiService.HandleRegisterAsync, ContentType = "application/json" },
                    new() { Path = "/api/auth/logout", Method = "POST", Handler = UserAuthApiService.HandleLogoutAsync, ContentType = "application/json" },
                    new() { Path = "/api/user", Method = "GET", Handler = UserAuthApiService.HandleGetUserInfoAsync, ContentType = "application/json" },
                    new() { Path = "/api/user/password", Method = "PUT", Handler = UserAuthApiService.HandleChangePasswordAsync, ContentType = "application/json" },
                    new() { Path = "/api/users", Method = "GET", Handler = UserAuthApiService.HandleGetAllUsersAsync, ContentType = "application/json", RequireAuth = true },
                    new() { Path = "/api/users", Method = "DELETE", Handler = UserAuthApiService.HandleDeleteUserAsync, ContentType = "application/json", RequireAuth = true },
                    
                    // Database Management API endpoints
                    new() { Path = "/api/db/setup", Method = "POST", Handler = DatabaseManagementApiService.HandleSetupDatabaseAsync, ContentType = "application/json" },
                    new() { Path = "/api/db/test", Method = "GET", Handler = DatabaseManagementApiService.HandleTestDatabaseConnectionAsync, ContentType = "application/json" }
                };

                // Register endpoints via loop
                EndpointRegistrar.RegisterEndpoints(app, endpoints);

                // Default handler (must be last)
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
    // ReSharper disable once UnusedParameter.Local
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
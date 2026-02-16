using CommonFramework.Aop.Attributes;
using LoggingService.Services;
using LoggingService.Enums;
using ClientApplication.Config;
using ClientApplication.Database;
using ClientApplication.Database.UserAuthentication;

namespace ClientApplication.App;

/// <summary>
/// Contains the main entry point of the application
/// </summary>
internal static class Program
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    [Log(LogLevel = LogLevel.Debug, LogMethodEntry = false)]
    private static async Task Main()
    {
        // Print Banner
        Banner.Banner.PrintBanner();

        // Print project root directory using Framework logging service
        LoggingServiceImpl.InstanceVal.LogDebug($"Project Root Directory: {EnvironmentPath.ProjectRootDirectory}");

        // Setup and verify database environment
        LoggingServiceImpl.InstanceVal.LogDebug("Starting database environment setup...");
        var databaseSetupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
        
        if (databaseSetupSuccess)
        {
            LoggingServiceImpl.InstanceVal.LogInformation("Database environment is ready for use.");
            
            // Test the connection
            var connectionTest = await DatabaseSetupUtility.TestDemoDatabaseConnectionAsync();
            if (connectionTest)
            {
                LoggingServiceImpl.InstanceVal.LogInformation("Database connection test successful.");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning("Database connection test failed, but setup completed.");
            }
        }
        else
        {
            LoggingServiceImpl.InstanceVal.LogError("Failed to setup database environment. Application may not function correctly.");
        }

        // Interactive user authentication
        if (databaseSetupSuccess)
        {
            LoggingServiceImpl.InstanceVal.LogDebug("Starting interactive user authentication...");
            await InteractiveUserAuthenticationAsync();
        }

        // Application logic can be added here
    }

    /// <summary>
    /// Interactive user authentication - prompts user for username and password
    /// </summary>
    private static async Task InteractiveUserAuthenticationAsync()
    {
        try
        {
            Console.WriteLine("\n=== User Authentication System ===");
            Console.WriteLine("Please enter your login information:");
                
            // Get username
            Console.Write("Username: ");
            var username = Console.ReadLine();
                
            // Get password
            Console.Write("Password: ");
            var password = ReadPassword();
                
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                LoggingServiceImpl.InstanceVal.LogWarning("Username or password cannot be empty");
                Console.WriteLine("Error: Username or password cannot be empty!");
                return;
            }
                
            // Query user in database
            LoggingServiceImpl.InstanceVal.LogDebug($"Verifying login information for user '{username}'...");
            var (success, userId, foundUsername) = await UserAuthenticationService.AuthenticateUserAsync(username, password);
                
            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User authentication successful: ID={userId}, Username={foundUsername}");
                Console.WriteLine($"✅ Login successful! Welcome back, {foundUsername} (User ID: {userId})");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User authentication failed: Username '{username}' does not exist or password is incorrect");
                Console.WriteLine("❌ Login failed! Username or password is incorrect.");
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error occurred during user verification: {ex.Message}");
            Console.WriteLine($"❌ Verification process error: {ex.Message}");
        }
    }
        
    /// <summary>
    /// Reads password from console without displaying it
    /// </summary>
    private static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();
        ConsoleKeyInfo key;
            
        do
        {
            key = Console.ReadKey(true);
                
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);
            
        Console.WriteLine();
        return password.ToString();
    }
}
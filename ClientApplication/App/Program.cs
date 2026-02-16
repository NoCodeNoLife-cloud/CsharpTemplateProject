using CommonFramework.Aop.Attributes;
using LoggingService.Services;
using LoggingService.Enums;
using ClientApplication.Config;
using ClientApplication.Database;
using ClientApplication.Database.UserAuthentication;
using MySqlConnector;

namespace ClientApplication.App;

/// <summary>
/// Contains the main entry point of the application
/// </summary>
internal static class Program
{
    private const string LoginPrompt = "Please enter your login information:";
    private const string UsernameField = "Username";
    private const string PasswordField = "Password";
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 50;
    private const int MinPasswordLength = 6;
    private const int MaxPasswordLength = 100;

    /// <summary>
    /// Entry point of the application
    /// </summary>
    [Log(LogLevel = LogLevel.Debug, LogMethodEntry = false)]
    [Obsolete("Obsolete")]
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
    /// Interactive user authentication - prompts user for login or registration choice
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task InteractiveUserAuthenticationAsync()
    {
        try
        {
            while (true)
            {
                // Display menu
                Console.WriteLine("\n=== User Authentication Menu ===");
                Console.WriteLine("1. Login with existing account");
                Console.WriteLine("2. Register new account");
                Console.WriteLine("3. Exit");
                Console.Write("Please select an option (1-3): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await HandleUserLoginAsync();
                        return; // Exit after successful login
                    case "2":
                        await HandleUserRegistrationAsync();
                        break; // Continue to show menu after registration
                    case "3":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please enter 1, 2, or 3.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user authentication: {ex.Message}");
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles user login process
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleUserLoginAsync()
    {
        try
        {
            Console.WriteLine("\n=== Login ===");
            Console.WriteLine(LoginPrompt);

            // Get username with validation
            var username = GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = GetPasswordInput();
            if (string.IsNullOrEmpty(password)) return;

            // Query user in database
            LoggingServiceImpl.InstanceVal.LogDebug($"Verifying login information for user '{username}'...");
            var (success, userId, foundUsername) = await UserAuthenticationService.AuthenticateUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User authentication successful: ID={userId}, Username={foundUsername}");
                Console.WriteLine($"Login successful! Welcome back, {foundUsername} (User ID: {userId})");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User authentication failed: Username '{username}' does not exist or password is incorrect");
                Console.WriteLine($"Login failed! Username or password is incorrect.");
            }
        }
        catch (MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during login: {dbEx.Message}");
            Console.WriteLine($"Database connection error: {dbEx.Message}");
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during login: {ioEx.Message}");
            Console.WriteLine($"Invalid operation: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user login: {ex.Message}");
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles user registration process
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleUserRegistrationAsync()
    {
        try
        {
            Console.WriteLine("\n=== Registration ===");
            Console.WriteLine("Please enter your registration information:");

            // Get username with validation
            var username = GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = GetPasswordInput();
            if (string.IsNullOrEmpty(password)) return;

            // Confirm password
            Console.Write("Confirm Password: ");
            var confirmPassword = ReadPasswordSecurely();

            if (password != confirmPassword)
            {
                Console.WriteLine("Passwords do not match!");
                return;
            }

            // Register user in database
            LoggingServiceImpl.InstanceVal.LogDebug($"Attempting to register new user '{username}'...");
            var (success, userId, errorMessage) = await UserAuthenticationService.RegisterUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User registration successful: ID={userId}, Username={username}");
                Console.WriteLine($"Registration successful! Your account has been created. (User ID: {userId})");
                Console.WriteLine("You can now login with your new account.");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User registration failed: {errorMessage}");
                Console.WriteLine($"Registration failed! {errorMessage}");
            }
        }
        catch (MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during registration: {dbEx.Message}");
            Console.WriteLine($"Database connection error: {dbEx.Message}");
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during registration: {ioEx.Message}");
            Console.WriteLine($"Invalid operation: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user registration: {ex.Message}");
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets user input with validation
    /// </summary>
    /// <param name="fieldName">Name of the field for display</param>
    /// <param name="minLength">Minimum length requirement</param>
    /// <param name="maxLength">Maximum length requirement</param>
    /// <returns>Validated input or null if canceled</returns>
    private static string GetUserInput(string fieldName, int minLength, int maxLength)
    {
        while (true)
        {
            Console.Write($"{fieldName} ({minLength}-{maxLength} characters): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"{fieldName} cannot be empty. Press Ctrl+C to exit.");
                continue;
            }

            if (input.Length < minLength)
            {
                Console.WriteLine($"{fieldName} must be at least {minLength} characters long.");
                continue;
            }

            if (input.Length <= maxLength) return input.Trim();
            Console.WriteLine($"{fieldName} cannot exceed {maxLength} characters.");
        }
    }

    /// <summary>
    /// Gets password input securely with validation
    /// </summary>
    /// <returns>Validated password or null if canceled</returns>
    private static string GetPasswordInput()
    {
        const int minPasswordLength = 6;
        const int maxPasswordLength = 100;

        while (true)
        {
            Console.Write($"{PasswordField} ({MinPasswordLength}-{MaxPasswordLength} characters): ");
            var password = ReadPasswordSecurely();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine($"Password cannot be empty. Press Ctrl+C to exit.");
                continue;
            }

            switch (password.Length)
            {
                case < minPasswordLength:
                    Console.WriteLine($"Password must be at least {MinPasswordLength} characters long.");
                    continue;
                case <= maxPasswordLength:
                    return password;
                default:
                    Console.WriteLine($"Password cannot exceed {MaxPasswordLength} characters.");
                    break;
            }
        }
    }

    /// <summary>
    /// Reads password from console securely without displaying it
    /// </summary>
    /// <returns>Password string</returns>
    private static string ReadPasswordSecurely()
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
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password.ToString();
    }
}
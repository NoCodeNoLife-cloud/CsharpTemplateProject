using CommonFramework.Aop.Attributes;
using System;
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
        try
        {
            // Print enhanced Banner
            Banner.Banner.PrintBanner();

            // Enhanced startup sequence
            await InitializeApplicationAsync();

            // Interactive user authentication
            await InteractiveUserAuthenticationAsync();
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Application startup failed: {ex.Message}", ex);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Initialize application components with progress indication
    /// </summary>
    private static async Task InitializeApplicationAsync()
    {
        LoggingServiceImpl.InstanceVal.LogDebug($"Project Root Directory: {EnvironmentPath.ProjectRootDirectory}");
        await Task.Delay(500); // Simulate work
        Console.WriteLine($"Environment configured");

        // Step 2: Database setup
        LoggingServiceImpl.InstanceVal.LogDebug("Starting database environment setup...");
        var databaseSetupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
        await Task.Delay(800); // Simulate work

        if (databaseSetupSuccess)
        {
            LoggingServiceImpl.InstanceVal.LogInformation("Database environment ready");
        }
        else
        {
            LoggingServiceImpl.InstanceVal.LogError("Database setup failed");
            throw new InvalidOperationException("Failed to setup database environment");
        }

        // Step 3: Connection test
        var connectionTest = await DatabaseSetupUtility.TestDemoDatabaseConnectionAsync();
        await Task.Delay(300); // Simulate work

        if (connectionTest)
        {
            LoggingServiceImpl.InstanceVal.LogInformation("Database connection established and test successful");
        }
        else
        {
            LoggingServiceImpl.InstanceVal.LogWarning("Database connection test failed, but setup completed");
        }

        // Step 4: Final initialization
        await Task.Delay(200); // Simulate work
        LoggingServiceImpl.InstanceVal.LogInformation("System initialization complete! Application is ready for use!");
        await Task.Delay(1000);
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
                // Display enhanced menu
                DisplayAuthenticationMenu();

                var choice = GetUserMenuChoice();

                switch (choice)
                {
                    case "1":
                        await HandleUserLoginAsync();
                        return; // Exit after successful login
                    case "2":
                        await HandleUserRegistrationAsync();
                        Console.WriteLine("\nPress Enter to return to main menu...");
                        Console.ReadLine();
                        break; // Continue to show menu after registration
                    default:
                        LoggingServiceImpl.InstanceVal.LogWarning("Invalid option selected!");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user authentication: {ex.Message}", ex);
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Displays the enhanced authentication menu
    /// </summary>
    private static void DisplayAuthenticationMenu()
    {
        Console.Write("[1] ");
        Console.WriteLine($"Login with existing account");

        Console.Write("[2] ");
        Console.WriteLine($"Register new account");

        Console.Write("[3] ");
        Console.WriteLine($"Exit application");
    }

    /// <summary>
    /// Gets and validates user menu choice
    /// </summary>
    /// <returns>User's menu choice</returns>
    private static string GetUserMenuChoice()
    {
        Console.Write("Enter your choice (1-3): ");
        var choice = Console.ReadLine()?.Trim();

        // Validate input
        if (string.IsNullOrEmpty(choice) || !IsValidMenuChoice(choice))
        {
            return "INVALID";
        }

        return choice;
    }

    /// <summary>
    /// Validates if the menu choice is valid
    /// </summary>
    /// <param name="choice">User's choice</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValidMenuChoice(string choice)
    {
        return choice switch
        {
            "1" or "2" or "3" => true,
            _ => false
        };
    }

    /// <summary>
    /// Handles user login process
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleUserLoginAsync()
    {
        try
        {
            Console.WriteLine($"{LoginPrompt}");

            // Get username with validation
            var username = GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = GetPasswordInput();
            if (string.IsNullOrEmpty(password)) return;

            // Show processing indicator
            LoggingServiceImpl.InstanceVal.LogDebug($"Verifying credentials for user '{username}'...");
            var (success, userId, foundUsername) = await UserAuthenticationService.AuthenticateUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User authentication successful: ID={userId}, Username={foundUsername}, welcome back");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User authentication failed: Username '{username}' does not exist or password is incorrect");
            }
        }
        catch (MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during login: {dbEx.Message}", dbEx);
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during login: {ioEx.Message}", ioEx);
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user login: {ex.Message}", ex);
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
            Console.Clear();
            Console.WriteLine($"Please enter your registration information:");

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
                LoggingServiceImpl.InstanceVal.LogError($"Passwords do not match!");
                Console.WriteLine("Press Enter to try again...");
                Console.ReadLine();
                return;
            }

            // Show registration progress
            LoggingServiceImpl.InstanceVal.LogDebug($"Attempting to register new user '{username}'...");
            var (success, userId, errorMessage) = await UserAuthenticationService.RegisterUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User registration successful: ID={userId}, Username={username}");
                Console.WriteLine($"Your account has been created. (User ID: {userId})");
                Console.WriteLine($"You can now login with your new account.");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User registration failed: {errorMessage}");
                Console.WriteLine($"Registration failed! {errorMessage}");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during registration: {dbEx.Message}", dbEx);
            Console.WriteLine($"Database connection error: {dbEx.Message}");
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during registration: {ioEx.Message}", ioEx);
            Console.WriteLine($"Invalid operation: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user registration: {ex.Message}", ex);
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
                Console.WriteLine($"{fieldName} cannot be empty.");
                Console.WriteLine($"Press Ctrl+C to exit or try again.");
                continue;
            }

            if (input.Length < minLength)
            {
                Console.WriteLine($"{fieldName} must be at least {minLength} characters long.");
                Console.WriteLine($"Current length: {input.Length} characters");
                continue;
            }

            if (input.Length <= maxLength) return input.Trim();
            Console.WriteLine($"{fieldName} cannot exceed {maxLength} characters.");
            Console.WriteLine($"Current length: {input.Length} characters");
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
                Console.WriteLine($"Password cannot be empty.");
                Console.WriteLine($"Press Ctrl+C to exit or try again.");
                continue;
            }

            switch (password.Length)
            {
                case < minPasswordLength:
                    Console.WriteLine($"Password must be at least {MinPasswordLength} characters long.");
                    Console.WriteLine($"Current length: {password.Length} characters");
                    continue;
                case > maxPasswordLength:
                    Console.WriteLine($"Password cannot exceed {MaxPasswordLength} characters.");
                    Console.WriteLine($"Current length: {password.Length} characters");
                    continue;
                default:
                    return password;
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
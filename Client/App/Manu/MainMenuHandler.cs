using Client.Database.UserAuthentication;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.App.Manu;

/// <summary>
/// Main menu handler for user interactions
/// </summary>
internal static class MainMenuHandler
{
    private const string UsernameField = "Username";
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 50;
    private const int MinPasswordLength = 6;
    private const int MaxPasswordLength = 100;

    /// <summary>
    /// Displays the main menu based on current login status
    /// </summary>
    public static void DisplayMainMenu()
    {
        LoggingFactory.Instance.LogInformation("Displaying main menu");
        Console.WriteLine("\n=== User Management System ===");

        // Check current login status to determine which options to show
        if (UserAuthenticationService.CurrentUserStatus == LoginStatus.NotLoggedIn)
        {
            // User not logged in - show login and registration options
            Console.Write("[1] ");
            Console.WriteLine($"Login with existing account");

            Console.Write("[2] ");
            Console.WriteLine($"Register new account");

            Console.Write("[3] ");
            Console.WriteLine($"Exit application");
        }
        else
        {
            // User logged in - show account management for all users
            Console.Write("[1] ");
            Console.WriteLine($"Account Management");

            // Show additional options based on permission level
            if (UserAuthenticationService.IsUserAdministrator())
            {
                // Administrator or Super Administrator - show admin operations
                Console.Write("[2] ");
                Console.WriteLine($"User Management (Admin Operations)");

                Console.Write("[3] ");
                Console.WriteLine($"Logout");

                Console.Write("[4] ");
                Console.WriteLine($"Exit application");
            }
            else
            {
                // Regular user - show logout and exit
                Console.Write("[2] ");
                Console.WriteLine($"Logout");

                Console.Write("[3] ");
                Console.WriteLine($"Exit application");
            }
        }

        Console.WriteLine("===============================");
    }

    /// <summary>
    /// Gets and validates user menu choice
    /// </summary>
    /// <returns>User's menu choice</returns>
    public static string GetUserMenuChoice()
    {
        LoggingFactory.Instance.LogDebug("Waiting for user menu choice input");
        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine()?.Trim();

        // Validate input
        return string.IsNullOrEmpty(choice) ? "INVALID" : choice;
    }

    /// <summary>
    /// Handles user login process
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleUserLoginAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("Please enter your login information:");

            // Get username with validation
            var username = InputValidator.GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = InputValidator.GetPasswordInput(MinPasswordLength, MaxPasswordLength);
            if (string.IsNullOrEmpty(password)) return;

            // Show processing indicator
            LoggingFactory.Instance.LogDebug($"Verifying credentials for user '{username}'...");
            var (success, userId, foundUsername) = await UserAuthenticationService.AuthenticateUserAsync(username, password);

            if (success)
            {
                LoggingFactory.Instance.LogInformation($"User authentication successful: ID={userId}, Username={foundUsername}, welcome back");
                LoggingFactory.Instance.LogInformation($"Welcome back, {foundUsername}! (User ID: {userId})");
            }
            else
            {
                LoggingFactory.Instance.LogWarning($"User authentication failed: Username '{username}' does not exist or password is incorrect");
                LoggingFactory.Instance.LogWarning("Authentication failed. Please check your credentials.");
            }
        }
        catch (MySqlConnector.MySqlException dbEx)
        {
            LoggingFactory.Instance.LogError($"Database error during login: {dbEx.Message}", dbEx);
            LoggingFactory.Instance.LogError($"Database connection error: {dbEx.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingFactory.Instance.LogError($"Invalid operation during login: {ioEx.Message}", ioEx);
            LoggingFactory.Instance.LogError($"Invalid operation: {ioEx.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Unexpected error during user login: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Unexpected error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles user registration process
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleUserRegistrationAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("Please select your account type first:");

            // Select user priority/permission level first
            var priority = GetUserPrioritySelection();
            if (string.IsNullOrEmpty(priority)) return;

            Console.Clear();
            LoggingFactory.Instance.LogInformation($"Creating new {priority} account:");

            // Get username with validation
            var username = InputValidator.GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = InputValidator.GetPasswordInput(MinPasswordLength, MaxPasswordLength);
            if (string.IsNullOrEmpty(password)) return;

            // Confirm password
            Console.Write("Confirm Password: ");
            var confirmPassword = InputValidator.ReadPasswordSecurely();

            if (password != confirmPassword)
            {
                LoggingFactory.Instance.LogError($"Passwords do not match!");
                LoggingFactory.Instance.LogWarning("Passwords do not match! Please try again.");
                await Task.Delay(2000); // Auto continue after showing error
                return;
            }

            // Show registration progress
            LoggingFactory.Instance.LogDebug($"Attempting to register new user '{username}' with {priority} permissions...");
            var (success, userId, errorMessage) = await UserAuthenticationService.RegisterUserAsync(username, password, priority);

            if (success)
            {
                LoggingFactory.Instance.LogInformation($"User registration successful: ID={userId}, Username={username}");
                LoggingFactory.Instance.LogInformation("Registration successful!");
                LoggingFactory.Instance.LogInformation($"Your account has been created. (User ID: {userId})");
                LoggingFactory.Instance.LogInformation("You can now login with your new account.");
                // Auto continue to main menu
                await Task.Delay(2000); // Give user time to read the message
            }
            else
            {
                LoggingFactory.Instance.LogWarning($"User registration failed: {errorMessage}");
                LoggingFactory.Instance.LogWarning($"Registration failed! {errorMessage}");
            }
        }
        catch (MySqlConnector.MySqlException dbEx)
        {
            LoggingFactory.Instance.LogError($"Database error during registration: {dbEx.Message}", dbEx);
            LoggingFactory.Instance.LogError($"Database connection error: {dbEx.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingFactory.Instance.LogError($"Invalid operation during registration: {ioEx.Message}", ioEx);
            LoggingFactory.Instance.LogError($"Invalid operation: {ioEx.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Unexpected error during user registration: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Unexpected error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Gets user priority selection from interactive menu
    /// </summary>
    /// <returns>Selected priority ("user" or "admin"), or null if canceled</returns>
    private static string? GetUserPrioritySelection()
    {
        try
        {
            Console.Clear();
            Console.WriteLine("=== Select Account Type ===");
            Console.WriteLine("Please choose the type of account you want to create:");
            Console.WriteLine("");

            Console.Write("[1] ");
            Console.WriteLine("Regular User (Standard permissions)");

            Console.Write("[2] ");
            Console.WriteLine("Administrator (Full permissions)");

            Console.WriteLine("");
            Console.Write("Enter your choice (1 or 2): ");

            while (true)
            {
                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        LoggingFactory.Instance.LogInformation("Selected: Regular User");
                        return "user";
                    case "2":
                        LoggingFactory.Instance.LogInformation("Selected: Administrator");
                        return "admin";
                    case "":
                        Console.WriteLine("Please make a selection.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please enter 1 or 2.");
                        break;
                }

                Console.Write("Enter your choice (1 or 2): ");
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error during priority selection: {ex.Message}", ex);
            return null;
        }
    }
}
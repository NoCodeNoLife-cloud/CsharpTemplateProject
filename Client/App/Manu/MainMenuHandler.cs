using Client.Database.UserAuthentication;
using LoggingService.Services;

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
    /// Displays the main menu
    /// </summary>
    public static void DisplayMainMenu()
    {
        LoggingServiceImpl.InstanceVal.LogInformation("Displaying main menu");
        Console.WriteLine("\n=== User Management System ===");
        Console.Write("[1] ");
        Console.WriteLine($"Login with existing account");

        Console.Write("[2] ");
        Console.WriteLine($"Register new account");

        Console.Write("[3] ");
        Console.WriteLine($"User Management (Admin Operations)");

        Console.Write("[4] ");
        Console.WriteLine($"Exit application");
        Console.WriteLine("===============================");
    }

    /// <summary>
    /// Gets and validates user menu choice
    /// </summary>
    /// <returns>User's menu choice</returns>
    public static string GetUserMenuChoice()
    {
        LoggingServiceImpl.InstanceVal.LogDebug("Waiting for user menu choice input");
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
            LoggingServiceImpl.InstanceVal.LogInformation("Please enter your login information:");

            // Get username with validation
            var username = InputValidator.GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = InputValidator.GetPasswordInput(MinPasswordLength, MaxPasswordLength);
            if (string.IsNullOrEmpty(password)) return;

            // Show processing indicator
            LoggingServiceImpl.InstanceVal.LogDebug($"Verifying credentials for user '{username}'...");
            var (success, userId, foundUsername) = await UserAuthenticationService.AuthenticateUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User authentication successful: ID={userId}, Username={foundUsername}, welcome back");
                LoggingServiceImpl.InstanceVal.LogInformation($"Welcome back, {foundUsername}! (User ID: {userId})");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User authentication failed: Username '{username}' does not exist or password is incorrect");
                LoggingServiceImpl.InstanceVal.LogWarning("Authentication failed. Please check your credentials.");
            }
        }
        catch (MySqlConnector.MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during login: {dbEx.Message}", dbEx);
            LoggingServiceImpl.InstanceVal.LogError($"Database connection error: {dbEx.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during login: {ioEx.Message}", ioEx);
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation: {ioEx.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user login: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
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
            LoggingServiceImpl.InstanceVal.LogInformation("Please enter your registration information:");

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
                LoggingServiceImpl.InstanceVal.LogError($"Passwords do not match!");
                LoggingServiceImpl.InstanceVal.LogWarning("Passwords do not match! Please try again.");
                LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            // Show registration progress
            LoggingServiceImpl.InstanceVal.LogDebug($"Attempting to register new user '{username}'...");
            var (success, userId, errorMessage) = await UserAuthenticationService.RegisterUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User registration successful: ID={userId}, Username={username}");
                LoggingServiceImpl.InstanceVal.LogInformation("Registration successful!");
                LoggingServiceImpl.InstanceVal.LogInformation($"Your account has been created. (User ID: {userId})");
                LoggingServiceImpl.InstanceVal.LogInformation("You can now login with your new account.");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User registration failed: {errorMessage}");
                LoggingServiceImpl.InstanceVal.LogWarning($"Registration failed! {errorMessage}");
            }
        }
        catch (MySqlConnector.MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during registration: {dbEx.Message}", dbEx);
            LoggingServiceImpl.InstanceVal.LogError($"Database connection error: {dbEx.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during registration: {ioEx.Message}", ioEx);
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation: {ioEx.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user registration: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
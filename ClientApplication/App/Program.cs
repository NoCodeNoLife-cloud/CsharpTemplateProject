using CommonFramework.Aop.Attributes;
using System;
using System.IO;
using System.Linq;
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
    private const string UserIdField = "User ID";

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

            // Interactive user management
            await InteractiveUserManagementAsync();
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Application startup failed: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to exit...");
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
        LoggingServiceImpl.InstanceVal.LogInformation("Environment configured");

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
    /// Interactive user management - main menu for all user operations
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task InteractiveUserManagementAsync()
    {
        try
        {
            while (true)
            {
                // Display main menu
                DisplayMainMenu();

                var choice = GetUserMenuChoice();

                switch (choice)
                {
                    case "1":
                        await HandleUserLoginAsync();
                        break;
                    case "2":
                        await HandleUserRegistrationAsync();
                        break;
                    case "3":
                        await HandleUserManagementMenuAsync();
                        break;
                    case "4":
                        LoggingServiceImpl.InstanceVal.LogInformation("Thank you for using our application. Goodbye!");
                        return; // Exit application
                    default:
                        LoggingServiceImpl.InstanceVal.LogWarning("Invalid option selected!");
                        break;
                }

                if (choice != "4")
                {
                    LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
                    Console.ReadLine();
                }
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user management: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to exit...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Displays the main menu
    /// </summary>
    private static void DisplayMainMenu()
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
    /// Displays the user management submenu
    /// </summary>
    private static void DisplayUserManagementMenu()
    {
        LoggingServiceImpl.InstanceVal.LogInformation("Displaying user management submenu");
        Console.WriteLine("\n=== User Management Operations ===");
        Console.Write("[1] ");
        Console.WriteLine($"View all users");

        Console.Write("[2] ");
        Console.WriteLine($"Find user by ID");

        Console.Write("[3] ");
        Console.WriteLine($"Find user by username");

        Console.Write("[4] ");
        Console.WriteLine($"Update user password");

        Console.Write("[5] ");
        Console.WriteLine($"Delete user account");

        Console.Write("[6] ");
        Console.WriteLine($"View statistics");

        Console.Write("[7] ");
        Console.WriteLine($"Back to main menu");
        Console.WriteLine("==================================");
    }

    /// <summary>
    /// Gets and validates user menu choice
    /// </summary>
    /// <returns>User's menu choice</returns>
    private static string GetUserMenuChoice()
    {
        LoggingServiceImpl.InstanceVal.LogDebug("Waiting for user menu choice input");
        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine()?.Trim();

        // Validate input
        if (string.IsNullOrEmpty(choice))
        {
            return "INVALID";
        }

        return choice;
    }

    /// <summary>
    /// Handles the user management submenu
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleUserManagementMenuAsync()
    {
        try
        {
            while (true)
            {
                DisplayUserManagementMenu();
                var choice = GetUserMenuChoice();

                switch (choice)
                {
                    case "1":
                        await HandleViewAllUsersAsync();
                        break;
                    case "2":
                        await HandleFindUserByIdAsync();
                        break;
                    case "3":
                        await HandleFindUserByUsernameAsync();
                        break;
                    case "4":
                        await HandleUpdateUserPasswordAsync();
                        break;
                    case "5":
                        await HandleDeleteUserAsync();
                        break;
                    case "6":
                        await HandleViewStatisticsAsync();
                        break;
                    case "7":
                        return; // Back to main menu
                    default:
                        LoggingServiceImpl.InstanceVal.LogWarning("Invalid option selected!");
                        break;
                }

                if (choice != "7")
                {
                    LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
                    Console.ReadLine();
                }
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error in user management menu: {ex.Message}", ex);
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
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation(LoginPrompt);

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
                LoggingServiceImpl.InstanceVal.LogInformation($"Welcome back, {foundUsername}! (User ID: {userId})");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User authentication failed: Username '{username}' does not exist or password is incorrect");
                LoggingServiceImpl.InstanceVal.LogWarning("Authentication failed. Please check your credentials.");
            }
        }
        catch (MySqlException dbEx)
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
    private static async Task HandleUserRegistrationAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("Please enter your registration information:");

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
        catch (MySqlException dbEx)
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
                LoggingServiceImpl.InstanceVal.LogWarning($"{fieldName} cannot be empty.");
                LoggingServiceImpl.InstanceVal.LogDebug($"Press Ctrl+C to exit or try again.");
                continue;
            }

            if (input.Length < minLength)
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"{fieldName} must be at least {minLength} characters long.");
                LoggingServiceImpl.InstanceVal.LogDebug($"Current length: {input.Length} characters");
                continue;
            }

            if (input.Length <= maxLength) return input.Trim();
            LoggingServiceImpl.InstanceVal.LogWarning($"{fieldName} cannot exceed {maxLength} characters.");
            LoggingServiceImpl.InstanceVal.LogDebug($"Current length: {input.Length} characters");
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
                LoggingServiceImpl.InstanceVal.LogWarning($"Password cannot be empty.");
                LoggingServiceImpl.InstanceVal.LogDebug($"Press Ctrl+C to exit or try again.");
                continue;
            }

            switch (password.Length)
            {
                case < minPasswordLength:
                    LoggingServiceImpl.InstanceVal.LogWarning($"Password must be at least {MinPasswordLength} characters long.");
                    LoggingServiceImpl.InstanceVal.LogDebug($"Current length: {password.Length} characters");
                    continue;
                case > maxPasswordLength:
                    LoggingServiceImpl.InstanceVal.LogWarning($"Password cannot exceed {MaxPasswordLength} characters.");
                    LoggingServiceImpl.InstanceVal.LogDebug($"Current length: {password.Length} characters");
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

    /// <summary>
    /// Gets user ID input with validation
    /// </summary>
    /// <returns>Validated user ID or 0 if invalid</returns>
    private static int GetUserIdInput()
    {
        while (true)
        {
            Console.Write($"{UserIdField}: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"{UserIdField} cannot be empty.");
                continue;
            }

            if (int.TryParse(input, out var userId) && userId > 0)
            {
                return userId;
            }

            LoggingServiceImpl.InstanceVal.LogWarning($"Please enter a valid positive integer for {UserIdField}.");
        }
    }

    /// <summary>
    /// Handles viewing all users
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleViewAllUsersAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("=== All Users ===");

            var users = await UserAuthenticationService.GetAllUsersAsync();
            var userList = users.ToList();

            if (userList.Count == 0)
            {
                LoggingServiceImpl.InstanceVal.LogWarning("No users found in the system.");
                return;
            }

            LoggingServiceImpl.InstanceVal.LogInformation($"\nTotal users: {userList.Count}\n");
            LoggingServiceImpl.InstanceVal.LogInformation($"{"ID",-5} {"Username",-20}");
            LoggingServiceImpl.InstanceVal.LogInformation(new string('-', 30));

            foreach (var user in userList)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"{user.Id,-5} {user.Username,-20}");
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error viewing all users: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles finding user by ID
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleFindUserByIdAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("=== Find User by ID ===");

            var userId = GetUserIdInput();
            var user = await UserAuthenticationService.GetUserByIdAsync(userId);

            if (user != null)
            {
                LoggingServiceImpl.InstanceVal.LogInformation("User found:");
                LoggingServiceImpl.InstanceVal.LogInformation($"ID: {user.Id}");
                LoggingServiceImpl.InstanceVal.LogInformation($"Username: {user.Username}");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User with ID {userId} not found.");
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error finding user by ID: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles finding user by username
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleFindUserByUsernameAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("=== Find User by Username ===");

            var username = GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Since we don't have a direct method in UserAuthenticationService, 
            // we'll use UserService directly
            var userService = new Database.Services.UserService();
            var user = await userService.FindByUsernameAsync(username);

            if (user != null)
            {
                LoggingServiceImpl.InstanceVal.LogInformation("User found:");
                LoggingServiceImpl.InstanceVal.LogInformation($"ID: {user.Id}");
                LoggingServiceImpl.InstanceVal.LogInformation($"Username: {user.Username}");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User with username '{username}' not found.");
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error finding user by username: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles updating user password
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleUpdateUserPasswordAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("=== Update User Password ===");

            var userId = GetUserIdInput();
            
            // Verify user exists
            var user = await UserAuthenticationService.GetUserByIdAsync(userId);
            if (user == null)
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User with ID {userId} not found.");
                return;
            }

            LoggingServiceImpl.InstanceVal.LogInformation($"Updating password for user: {user.Username} (ID: {userId})");

            // Get new password
            var newPassword = GetPasswordInput();
            if (string.IsNullOrEmpty(newPassword)) return;

            // Confirm new password
            Console.Write("Confirm New Password: ");
            var confirmNewPassword = ReadPasswordSecurely();

            if (newPassword != confirmNewPassword)
            {
                LoggingServiceImpl.InstanceVal.LogWarning("New passwords do not match!");
                return;
            }

            var success = await UserAuthenticationService.UpdateUserPasswordAsync(userId, newPassword);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"Password updated successfully for user '{user.Username}' (ID: {userId})");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogError($"Failed to update password for user ID {userId}");
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error updating user password: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
    }



    /// <summary>
    /// Handles deleting user account
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleDeleteUserAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogWarning("=== Delete User Account ===");

            var userId = GetUserIdInput();
            
            // Verify user exists
            var user = await UserAuthenticationService.GetUserByIdAsync(userId);
            if (user == null)
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User with ID {userId} not found.");
                return;
            }

            LoggingServiceImpl.InstanceVal.LogWarning($"WARNING: This will permanently delete user '{user.Username}' (ID: {userId})");
            LoggingServiceImpl.InstanceVal.LogWarning("This action cannot be undone!");
            LoggingServiceImpl.InstanceVal.LogDebug("\nAre you absolutely sure? Type 'DELETE' to confirm: ");
            Console.Write("\nAre you absolutely sure? Type 'DELETE' to confirm: ");
            var confirmation = Console.ReadLine()?.Trim();

            if (confirmation != "DELETE")
            {
                LoggingServiceImpl.InstanceVal.LogInformation("Operation cancelled.");
                return;
            }

            var success = await UserAuthenticationService.DeleteUserAsync(userId);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User '{user.Username}' (ID: {userId}) deleted successfully");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogError($"Failed to delete user ID {userId}");
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error deleting user: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles viewing system statistics
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleViewStatisticsAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("=== System Statistics ===");

            // Get various statistics
            var totalUsers = await UserAuthenticationService.GetAllUsersAsync();
            var totalUsersCount = totalUsers.Count();

            LoggingServiceImpl.InstanceVal.LogInformation($"\n📊 User Statistics:");
            LoggingServiceImpl.InstanceVal.LogInformation($"   Total Users: {totalUsersCount}");

            // Show all users
            if (totalUsers.Any())
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"\n📋 All Users:");
                LoggingServiceImpl.InstanceVal.LogInformation($"{"ID",-5} {"Username",-20}");
                LoggingServiceImpl.InstanceVal.LogInformation(new string('-', 30));
                foreach (var user in totalUsers)
                {
                    LoggingServiceImpl.InstanceVal.LogInformation($"{user.Id,-5} {user.Username,-20}");
                }
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error viewing statistics: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogError($"Error: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
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
            ConsoleHelper.PrintError($"Application startup failed: {ex.Message}");
            LoggingServiceImpl.InstanceVal.LogError($"Application startup failed: {ex.Message}", ex);
            ConsoleHelper.WaitForInput("Press Enter to exit...");
        }
    }

    /// <summary>
    /// Initialize application components with progress indication
    /// </summary>
    private static async Task InitializeApplicationAsync()
    {
        ConsoleHelper.PrintHeader("System Initialization", ConsoleHelper.MessageType.Info);

        // Progress tracking
        int totalSteps = 4;
        int currentStep = 0;

        // Step 1: Environment setup
        currentStep++;
        ConsoleHelper.PrintProgress(currentStep, totalSteps, "Setting up environment...");
        LoggingServiceImpl.InstanceVal.LogDebug($"Project Root Directory: {EnvironmentPath.ProjectRootDirectory}");
        await Task.Delay(500); // Simulate work
        ConsoleHelper.PrintSuccess("Environment configured");

        // Step 2: Database setup
        currentStep++;
        ConsoleHelper.PrintProgress(currentStep, totalSteps, "Initializing database...");
        LoggingServiceImpl.InstanceVal.LogDebug("Starting database environment setup...");
        var databaseSetupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
        await Task.Delay(800); // Simulate work

        if (databaseSetupSuccess)
        {
            ConsoleHelper.PrintSuccess("Database environment ready");
        }
        else
        {
            ConsoleHelper.PrintError("Database setup failed");
            throw new InvalidOperationException("Failed to setup database environment");
        }

        // Step 3: Connection test
        currentStep++;
        ConsoleHelper.PrintProgress(currentStep, totalSteps, "Testing database connection...");
        var connectionTest = await DatabaseSetupUtility.TestDemoDatabaseConnectionAsync();
        await Task.Delay(300); // Simulate work

        if (connectionTest)
        {
            ConsoleHelper.PrintSuccess("Database connection established");
            LoggingServiceImpl.InstanceVal.LogInformation("Database connection test successful.");
        }
        else
        {
            ConsoleHelper.PrintWarning("Database connection test failed, but setup completed");
            LoggingServiceImpl.InstanceVal.LogWarning("Database connection test failed, but setup completed.");
        }

        // Step 4: Final initialization
        currentStep++;
        ConsoleHelper.PrintProgress(currentStep, totalSteps, "Finalizing setup...");
        await Task.Delay(200); // Simulate work
        ConsoleHelper.PrintSuccess("System initialization complete!");

        ConsoleHelper.PrintSeparator();
        ConsoleHelper.PrintHighlight("Application is ready for use!");
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
                        ConsoleHelper.WaitForInput("\nPress Enter to return to main menu...");
                        break; // Continue to show menu after registration
                    case "3":
                        ConsoleHelper.PrintHeader("Thank You for Using Our Application", ConsoleHelper.MessageType.Success);
                        ConsoleHelper.PrintInfo("Goodbye! Have a great day!");
                        await ConsoleHelper.CountdownAsync(3, "Application closing in");
                        return;
                    default:
                        ConsoleHelper.PrintError("Invalid option selected!");
                        ConsoleHelper.PrintInfo("Please enter 1, 2, or 3.");
                        ConsoleHelper.WaitForInput("Press Enter to continue...");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user authentication: {ex.Message}");
            ConsoleHelper.PrintError($"Unexpected error: {ex.Message}");
            ConsoleHelper.WaitForInput("Press Enter to exit...");
        }
    }

    /// <summary>
    /// Displays the enhanced authentication menu
    /// </summary>
    private static void DisplayAuthenticationMenu()
    {
        ConsoleHelper.ClearScreenWithHeader("User Authentication Center");

        ConsoleHelper.PrintHighlight("Please select an option below:");
        ConsoleHelper.PrintSeparator(40);

        ConsoleHelper.PrintPrompt("[1] ");
        ConsoleHelper.PrintInfo("Login with existing account");

        ConsoleHelper.PrintPrompt("[2] ");
        ConsoleHelper.PrintInfo("Register new account");

        ConsoleHelper.PrintPrompt("[3] ");
        ConsoleHelper.PrintInfo("Exit application");

        ConsoleHelper.PrintSeparator(40);
    }

    /// <summary>
    /// Gets and validates user menu choice
    /// </summary>
    /// <returns>User's menu choice</returns>
    private static string GetUserMenuChoice()
    {
        ConsoleHelper.PrintPrompt("Enter your choice (1-3): ");
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
            ConsoleHelper.ClearScreenWithHeader("Account Login");
            ConsoleHelper.PrintInfo(LoginPrompt);
            ConsoleHelper.PrintSeparator();

            // Get username with validation
            var username = GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = GetPasswordInput();
            if (string.IsNullOrEmpty(password)) return;

            // Show processing indicator
            ConsoleHelper.PrintInfo("\nVerifying credentials...");

            // Query user in database
            LoggingServiceImpl.InstanceVal.LogDebug($"Verifying login information for user '{username}'...");
            var (success, userId, foundUsername) = await UserAuthenticationService.AuthenticateUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User authentication successful: ID={userId}, Username={foundUsername}");
                ConsoleHelper.PrintSuccess($"Login successful! Welcome back, {foundUsername}!");
                ConsoleHelper.PrintHighlight($"User ID: {userId}");
                ConsoleHelper.WaitForInput("\nPress Enter to continue to application...");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User authentication failed: Username '{username}' does not exist or password is incorrect");
                ConsoleHelper.PrintError("Login failed! Username or password is incorrect.");
                ConsoleHelper.WaitForInput("\nPress Enter to return to menu...");
            }
        }
        catch (MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during login: {dbEx.Message}");
            ConsoleHelper.PrintError($"Database connection error: {dbEx.Message}");
            ConsoleHelper.WaitForInput("Press Enter to continue...");
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during login: {ioEx.Message}");
            ConsoleHelper.PrintError($"Invalid operation: {ioEx.Message}");
            ConsoleHelper.WaitForInput("Press Enter to continue...");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user login: {ex.Message}");
            ConsoleHelper.PrintError($"Unexpected error: {ex.Message}");
            ConsoleHelper.WaitForInput("Press Enter to continue...");
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
            ConsoleHelper.ClearScreenWithHeader("New Account Registration");
            ConsoleHelper.PrintInfo("Please enter your registration information:");
            ConsoleHelper.PrintSeparator();

            // Get username with validation
            var username = GetUserInput(UsernameField, MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(username)) return;

            // Get password with validation
            var password = GetPasswordInput();
            if (string.IsNullOrEmpty(password)) return;

            // Confirm password
            ConsoleHelper.PrintPrompt("Confirm Password: ");
            var confirmPassword = ReadPasswordSecurely();

            if (password != confirmPassword)
            {
                ConsoleHelper.PrintError("Passwords do not match!");
                ConsoleHelper.WaitForInput("Press Enter to try again...");
                return;
            }

            // Show registration progress
            ConsoleHelper.PrintInfo("\nCreating your account...");

            // Register user in database
            LoggingServiceImpl.InstanceVal.LogDebug($"Attempting to register new user '{username}'...");
            var (success, userId, errorMessage) = await UserAuthenticationService.RegisterUserAsync(username, password);

            if (success)
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"User registration successful: ID={userId}, Username={username}");
                ConsoleHelper.PrintSuccess("Registration successful!");
                ConsoleHelper.PrintHighlight($"Your account has been created. (User ID: {userId})");
                ConsoleHelper.PrintInfo("You can now login with your new account.");
            }
            else
            {
                LoggingServiceImpl.InstanceVal.LogWarning($"User registration failed: {errorMessage}");
                ConsoleHelper.PrintError($"Registration failed! {errorMessage}");
            }

            ConsoleHelper.WaitForInput("\nPress Enter to continue...");
        }
        catch (MySqlException dbEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Database error during registration: {dbEx.Message}");
            ConsoleHelper.PrintError($"Database connection error: {dbEx.Message}");
            ConsoleHelper.WaitForInput("Press Enter to continue...");
        }
        catch (InvalidOperationException ioEx)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Invalid operation during registration: {ioEx.Message}");
            ConsoleHelper.PrintError($"Invalid operation: {ioEx.Message}");
            ConsoleHelper.WaitForInput("Press Enter to continue...");
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during user registration: {ex.Message}");
            ConsoleHelper.PrintError($"Unexpected error: {ex.Message}");
            ConsoleHelper.WaitForInput("Press Enter to continue...");
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
            ConsoleHelper.PrintPrompt($"{fieldName} ({minLength}-{maxLength} characters): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                ConsoleHelper.PrintWarning($"{fieldName} cannot be empty.");
                ConsoleHelper.PrintInfo("Press Ctrl+C to exit or try again.");
                continue;
            }

            if (input.Length < minLength)
            {
                ConsoleHelper.PrintWarning($"{fieldName} must be at least {minLength} characters long.");
                ConsoleHelper.PrintInfo($"Current length: {input.Length} characters");
                continue;
            }

            if (input.Length <= maxLength) return input.Trim();
            ConsoleHelper.PrintWarning($"{fieldName} cannot exceed {maxLength} characters.");
            ConsoleHelper.PrintInfo($"Current length: {input.Length} characters");
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
            ConsoleHelper.PrintPrompt($"{PasswordField} ({MinPasswordLength}-{MaxPasswordLength} characters): ");
            var password = ReadPasswordSecurely();

            if (string.IsNullOrEmpty(password))
            {
                ConsoleHelper.PrintWarning("Password cannot be empty.");
                ConsoleHelper.PrintInfo("Press Ctrl+C to exit or try again.");
                continue;
            }

            switch (password.Length)
            {
                case < minPasswordLength:
                    ConsoleHelper.PrintWarning($"Password must be at least {MinPasswordLength} characters long.");
                    ConsoleHelper.PrintInfo($"Current length: {password.Length} characters");
                    continue;
                case > maxPasswordLength:
                    ConsoleHelper.PrintWarning($"Password cannot exceed {MaxPasswordLength} characters.");
                    ConsoleHelper.PrintInfo($"Current length: {password.Length} characters");
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
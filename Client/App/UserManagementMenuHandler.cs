using Client.App.Manu;
using Client.App.Services;
using Client.Database.UserAuthentication;
using Common.Models;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.App;

/// <summary>
/// User management submenu handler
/// </summary>
internal static class UserManagementMenuHandler
{
    private const string UserIdField = "User ID";

    /// <summary>
    /// Displays the user management submenu
    /// </summary>
    public static void DisplayUserManagementMenu()
    {
        LoggingFactory.Instance.LogInformation("Displaying user management submenu");
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
    /// Handles the user management submenu
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleUserManagementMenuAsync()
    {
        // Security check - ensure only administrators can access this menu
        if (!UserAuthenticationService.IsUserAdministrator())
        {
            LoggingFactory.Instance.LogWarning("Access denied: User management requires administrator privileges");
            Console.WriteLine("Access denied: This feature requires administrator privileges.");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            return;
        }

        try
        {
            while (true)
            {
                DisplayUserManagementMenu();
                var choice = MainMenuHandler.GetUserMenuChoice();

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
                        LoggingFactory.Instance.LogWarning("Invalid option selected!");
                        break;
                }

                // No need for continue here as it's the last statement in the loop
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error in user management menu: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Handles viewing all users
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleViewAllUsersAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== All Users ===");

            // Use Server API to get all users (requires admin API key)
            var users = await ServerAuthService.GetAllUsersAsync(ServerAuthService.DefaultApiKey);
            var userList = users?.ToList() ?? [];

            if (userList.Count == 0)
            {
                LoggingFactory.Instance.LogWarning("No users found in the system or API call failed.");
                return;
            }

            LoggingFactory.Instance.LogInformation($"\nTotal users: {userList.Count}\n");
            LoggingFactory.Instance.LogInformation($"{"ID",-5} {"Username",-20}");
            LoggingFactory.Instance.LogInformation(new string('-', 30));

            foreach (var user in userList)
            {
                LoggingFactory.Instance.LogInformation($"{user.Id,-5} {user.Username,-20}");
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error viewing all users: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles finding user by ID
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleFindUserByIdAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== Find User by ID ===");

            var userId = GetUserIdInput();

            // Use Server API to get user by ID
            var user = await ServerAuthService.GetUserInfoAsync(userId);

            if (user != null)
            {
                LoggingFactory.Instance.LogInformation("User found:");
                LoggingFactory.Instance.LogInformation($"ID: {user.Id}");
                LoggingFactory.Instance.LogInformation($"Username: {user.Username}");
                await Task.Delay(1500); // Auto continue after showing user info
            }
            else
            {
                LoggingFactory.Instance.LogWarning($"User with ID {userId} not found.");
                await Task.Delay(1500); // Auto continue after showing not found message
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error finding user by ID: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles finding user by username
    /// </summary>
    public static async Task HandleFindUserByUsernameAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== Find User by Username ===");

            var username = InputValidator.GetUserInput("Username", 3, 50);
            if (string.IsNullOrEmpty(username)) return;

            // Get all users from Server API and filter by username
            var allUsers = await ServerAuthService.GetAllUsersAsync(ServerAuthService.DefaultApiKey);
            var user = allUsers?.FirstOrDefault(u => u.Username != null && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                LoggingFactory.Instance.LogInformation("User found:");
                LoggingFactory.Instance.LogInformation($"ID: {user.Id}");
                LoggingFactory.Instance.LogInformation($"Username: {user.Username}");
                await Task.Delay(1500); // Auto continue after showing user info
            }
            else
            {
                LoggingFactory.Instance.LogWarning($"User with username '{username}' not found.");
                await Task.Delay(1500); // Auto continue after showing not found message
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error finding user by username: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles updating user password
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleUpdateUserPasswordAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== Update User Password ===");

            var userId = GetUserIdInput();

            // Verify user exists via Server API
            var user = await ServerAuthService.GetUserInfoAsync(userId);
            if (user == null)
            {
                LoggingFactory.Instance.LogWarning($"User with ID {userId} not found.");
                return;
            }

            LoggingFactory.Instance.LogInformation($"Updating password for user: {user.Username} (ID: {userId})");

            // Note: Admin changing password for another user doesn't need current password
            // Get new password
            var newPassword = InputValidator.GetPasswordInput(6, 100);
            if (string.IsNullOrEmpty(newPassword)) return;

            // Confirm new password
            Console.Write("Confirm New Password: ");
            var confirmNewPassword = InputValidator.ReadPasswordSecurely();

            if (newPassword != confirmNewPassword)
            {
                LoggingFactory.Instance.LogWarning("New passwords do not match!");
                return;
            }

            // For admin password reset, we can use a special flag or bypass current password check
            // This would require Server API support for admin password reset
            LoggingFactory.Instance.LogWarning("Admin password reset requires Server API enhancement.");
            LoggingFactory.Instance.LogWarning("For now, this operation is not fully implemented.");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error updating user password: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles deleting user account
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleDeleteUserAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogWarning("=== Delete User Account ===");

            var userId = GetUserIdInput();

            // Verify user exists via Server API
            var user = await ServerAuthService.GetUserInfoAsync(userId);
            if (user == null)
            {
                LoggingFactory.Instance.LogWarning($"User with ID {userId} not found.");
                return;
            }

            LoggingFactory.Instance.LogWarning($"WARNING: This will permanently delete user '{user.Username}' (ID: {userId})");
            LoggingFactory.Instance.LogWarning("This action cannot be undone!");
            LoggingFactory.Instance.LogDebug("\nAre you absolutely sure? Type 'DELETE' to confirm: ");
            Console.Write("\nAre you absolutely sure? Type 'DELETE' to confirm: ");
            var confirmation = Console.ReadLine()?.Trim();

            if (confirmation != "DELETE")
            {
                LoggingFactory.Instance.LogInformation("Operation cancelled.");
                return;
            }

            // Use Server API to delete user (requires admin API key)
            var success = await ServerAuthService.DeleteUserAsync(userId, ServerAuthService.DefaultApiKey);

            if (success)
            {
                LoggingFactory.Instance.LogInformation($"Server API User '{user.Username}' (ID: {userId}) deleted successfully");
                await Task.Delay(1500); // Auto continue after success
            }
            else
            {
                LoggingFactory.Instance.LogError($"Server API failed to delete user ID {userId}");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine(); // Error case still needs confirmation
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error deleting user: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles viewing system statistics
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleViewStatisticsAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== System Statistics ===");

            // Get various statistics via Server API
            var totalUsers = await ServerAuthService.GetAllUsersAsync(ServerAuthService.DefaultApiKey);
            var enumerable = totalUsers?.ToArray() ?? Array.Empty<UserInfo>();
            var totalUsersCount = enumerable.Length;

            LoggingFactory.Instance.LogInformation($"\n📊 User Statistics:");
            LoggingFactory.Instance.LogInformation($"   Total Users: {totalUsersCount}");
            await Task.Delay(1000); // Brief pause before showing user list

            // Show all users
            if (enumerable.Any())
            {
                LoggingFactory.Instance.LogInformation($"\n📋 All Users:");
                LoggingFactory.Instance.LogInformation($"{"ID",-5} {"Username",-20}");
                LoggingFactory.Instance.LogInformation(new string('-', 30));
                foreach (var user in enumerable)
                {
                    LoggingFactory.Instance.LogInformation($"{user.Id,-5} {user.Username,-20}");
                }
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error viewing statistics: {ex.Message}", ex);
            LoggingFactory.Instance.LogError($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
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
                LoggingFactory.Instance.LogWarning($"{UserIdField} cannot be empty.");
                continue;
            }

            if (int.TryParse(input, out var userId) && userId > 0)
            {
                return userId;
            }

            LoggingFactory.Instance.LogWarning($"Please enter a valid positive integer for {UserIdField}.");
        }
    }
}
using Client.App.Manu;
using Client.Database.Models;
using Client.Database.Services;
using Client.Database.UserAuthentication;
using LoggingService.Services;

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
    /// Handles the user management submenu
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleUserManagementMenuAsync()
    {
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
                        LoggingServiceImpl.InstanceVal.LogWarning("Invalid option selected!");
                        break;
                }

                if (choice == "7") continue;
                LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to continue...");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Error in user management menu: {ex.Message}", ex);
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
    public static async Task HandleFindUserByIdAsync()
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
    public static async Task HandleFindUserByUsernameAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("=== Find User by Username ===");

            var username = InputValidator.GetUserInput("Username", 3, 50);
            if (string.IsNullOrEmpty(username)) return;

            // Since we don't have a direct method in UserAuthenticationService, 
            // we'll use UserService directly
            var userService = new Database.Services.UserService();
            var user = await UserService.FindByUsernameAsync(username);

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
    public static async Task HandleUpdateUserPasswordAsync()
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
            var newPassword = InputValidator.GetPasswordInput(6, 100);
            if (string.IsNullOrEmpty(newPassword)) return;

            // Confirm new password
            Console.Write("Confirm New Password: ");
            var confirmNewPassword = InputValidator.ReadPasswordSecurely();

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
    public static async Task HandleDeleteUserAsync()
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
    public static async Task HandleViewStatisticsAsync()
    {
        try
        {
            Console.Clear();
            LoggingServiceImpl.InstanceVal.LogInformation("=== System Statistics ===");

            // Get various statistics
            var totalUsers = await UserAuthenticationService.GetAllUsersAsync();
            var enumerable = totalUsers as User[] ?? totalUsers.ToArray();
            var totalUsersCount = enumerable.Count();

            LoggingServiceImpl.InstanceVal.LogInformation($"\n📊 User Statistics:");
            LoggingServiceImpl.InstanceVal.LogInformation($"   Total Users: {totalUsersCount}");

            // Show all users
            if (enumerable.Any())
            {
                LoggingServiceImpl.InstanceVal.LogInformation($"\n📋 All Users:");
                LoggingServiceImpl.InstanceVal.LogInformation($"{"ID",-5} {"Username",-20}");
                LoggingServiceImpl.InstanceVal.LogInformation(new string('-', 30));
                foreach (var user in enumerable)
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
}
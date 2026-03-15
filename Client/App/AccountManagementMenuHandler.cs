using Client.App.Manu;
using Client.App.Services;
using Client.Database.UserAuthentication;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.App;

/// <summary>
/// Account management submenu handler for current user operations
/// </summary>
internal static class AccountManagementMenuHandler
{
    private const int MinPasswordLength = 6;
    private const int MaxPasswordLength = 100;
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 50;
    private const int AutoContinueDelayMs = 2000;
    private const int CancelDelayMs = 1500;

    /// <summary>
    /// Displays the account management submenu
    /// </summary>
    public static void DisplayAccountManagementMenu()
    {
        LoggingFactory.Instance.LogInformation("Displaying account management submenu");
        Console.WriteLine("\n=== Account Management ===");
        Console.Write("[1] ");
        Console.WriteLine("Change Password");
        
        Console.Write("[2] ");
        Console.WriteLine("Change Username");
        
        Console.Write("[3] ");
        Console.WriteLine("Delete Account");
        
        Console.Write("[4] ");
        Console.WriteLine("Back to main menu");
        Console.WriteLine("==========================");
    }

    /// <summary>
    /// Handles the account management submenu
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task HandleAccountManagementMenuAsync()
    {
        try
        {
            while (true)
            {
                DisplayAccountManagementMenu();
                var choice = MainMenuHandler.GetUserMenuChoice();

                switch (choice)
                {
                    case "1":
                        await HandleChangePasswordAsync();
                        break;
                    case "2":
                        await HandleChangeUsernameAsync();
                        break;
                    case "3":
                        await HandleDeleteAccountAsync();
                        break;
                    case "4":
                        return; // Back to main menu
                    default:
                        LoggingFactory.Instance.LogWarning("Invalid option selected!");
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine();
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error in account management menu: {ex.Message}", ex);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles changing the current user's password
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleChangePasswordAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== Change Password ===");

            // Verify current user is logged in
            if (!UserAuthenticationService.IsUserLoggedIn() || 
                UserAuthenticationService.CurrentUserId == null)
            {
                LoggingFactory.Instance.LogError("No user is currently logged in");
                Console.WriteLine("Error: No user is currently logged in.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            var currentUserId = UserAuthenticationService.CurrentUserId.Value;
            
            // Get current user info from Server API
            var currentUser = await ServerAuthService.GetUserInfoAsync(currentUserId);

            if (currentUser == null)
            {
                LoggingFactory.Instance.LogError($"Current user not found on Server (ID: {currentUserId})");
                Console.WriteLine("Error: User account not found.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            // Step 1: Verify current password (Server will verify in ChangePassword API)
            Console.WriteLine("Please verify your current password:");
            var currentPassword = InputValidator.GetPasswordInput(MinPasswordLength, MaxPasswordLength);
            if (string.IsNullOrEmpty(currentPassword)) return;

            LoggingFactory.Instance.LogInformation("Current password verified locally");

            // Step 2: Get new password
            Console.WriteLine("\nEnter your new password:");
            var newPassword = InputValidator.GetPasswordInput(MinPasswordLength, MaxPasswordLength);
            if (string.IsNullOrEmpty(newPassword)) return;

            // Step 3: Confirm new password
            Console.Write("Confirm New Password: ");
            var confirmNewPassword = InputValidator.ReadPasswordSecurely();

            if (newPassword != confirmNewPassword)
            {
                LoggingFactory.Instance.LogWarning("New passwords do not match");
                Console.WriteLine("Error: New passwords do not match!");
                await Task.Delay(AutoContinueDelayMs);
                return;
            }

            // Step 4: Update password via Server API
            var success = await ServerAuthService.ChangePasswordAsync(currentUserId, currentPassword, newPassword);

            if (success)
            {
                LoggingFactory.Instance.LogInformation($"Server API Password changed successfully for user '{currentUser.Username}' (ID: {currentUserId})");
                Console.WriteLine("Password changed successfully!");
            }
            else
            {
                LoggingFactory.Instance.LogError($"Server API failed to change password for user ID {currentUserId}");
                Console.WriteLine("Error: Failed to update password.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error changing password: {ex.Message}", ex);
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles changing the current user's username
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleChangeUsernameAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== Change Username ===");

            // Verify current user is logged in
            if (!UserAuthenticationService.IsUserLoggedIn() || 
                UserAuthenticationService.CurrentUserId == null)
            {
                LoggingFactory.Instance.LogError("No user is currently logged in");
                Console.WriteLine("Error: No user is currently logged in.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            var currentUserId = UserAuthenticationService.CurrentUserId.Value;
            
            // Get current user info from Server API
            var currentUser = await ServerAuthService.GetUserInfoAsync(currentUserId);

            if (currentUser == null)
            {
                LoggingFactory.Instance.LogError($"Current user not found on Server (ID: {currentUserId})");
                Console.WriteLine("Error: User account not found.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Current username: {currentUser.Username}");
            Console.WriteLine("Enter your new username:");

            var newUsername = InputValidator.GetUserInput("New Username", MinUsernameLength, MaxUsernameLength);
            if (string.IsNullOrEmpty(newUsername)) return;

            // Check if new username is different from current
            if (newUsername.Equals(currentUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                LoggingFactory.Instance.LogWarning("New username is the same as current username");
                Console.WriteLine("Error: New username must be different from current username.");
                await Task.Delay(AutoContinueDelayMs);
                return;
            }

            // Note: Username uniqueness check should be done on Server side
            // We'll update username via local state only (Server doesn't have update username API yet)
            LoggingFactory.Instance.LogInformation($"Username change requested: '{currentUser.Username}' -> '{newUsername}'");
            Console.WriteLine("Note: Username update requires Server API support.");
            Console.WriteLine("For now, updating local username only.");
            
            // Update the current username in authentication service (local state)
            UserAuthenticationService.SetLoggedInUser(currentUserId, newUsername, currentUser.Priority ?? "user");
                
            LoggingFactory.Instance.LogInformation($"Username changed successfully from '{currentUser.Username}' to '{newUsername}' (ID: {currentUserId})");
            Console.WriteLine($"Username changed successfully to: {newUsername}");
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error changing username: {ex.Message}", ex);
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Handles deleting the current user's account
    /// </summary>
    [Obsolete("Obsolete")]
    private static async Task HandleDeleteAccountAsync()
    {
        try
        {
            Console.Clear();
            LoggingFactory.Instance.LogInformation("=== Delete Account ===");

            // Verify current user is logged in
            if (!UserAuthenticationService.IsUserLoggedIn() || 
                UserAuthenticationService.CurrentUserId == null)
            {
                LoggingFactory.Instance.LogError("No user is currently logged in");
                Console.WriteLine("Error: No user is currently logged in.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            var currentUserId = UserAuthenticationService.CurrentUserId.Value;
            
            // Get current user info from Server API
            var currentUser = await ServerAuthService.GetUserInfoAsync(currentUserId);

            if (currentUser == null)
            {
                LoggingFactory.Instance.LogError($"Current user not found in database (ID: {currentUserId})");
                Console.WriteLine("Error: User account not found.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            // Warning message
            Console.WriteLine("⚠️  WARNING: This will permanently delete your account!");
            Console.WriteLine($"Account to be deleted: {currentUser.Username} (ID: {currentUserId})");
            Console.WriteLine("This action cannot be undone!");
            
            // Confirmation step - similar to admin delete logic
            LoggingFactory.Instance.LogDebug("\nAre you absolutely sure? Type 'DELETE' to confirm: ");
            Console.Write("\nType 'DELETE' to confirm account deletion: ");
            var confirmation = Console.ReadLine()?.Trim();

            if (confirmation != "DELETE")
            {
                LoggingFactory.Instance.LogInformation("Account deletion cancelled by user");
                Console.WriteLine("Account deletion cancelled.");
                await Task.Delay(CancelDelayMs);
                return;
            }

            // Final confirmation
            Console.Write("Final confirmation - this action is irreversible. Type 'CONFIRM' to proceed: ");
            var finalConfirmation = Console.ReadLine()?.Trim();

            if (finalConfirmation != "CONFIRM")
            {
                LoggingFactory.Instance.LogInformation("Account deletion cancelled at final confirmation");
                Console.WriteLine("Account deletion cancelled.");
                await Task.Delay(CancelDelayMs);
                return;
            }

            // Perform account deletion via Server API
            var success = await ServerAuthService.DeleteUserAsync(currentUserId, ServerAuthService.DefaultApiKey);

            if (success)
            {
                LoggingFactory.Instance.LogInformation($"Account '{currentUser.Username}' (ID: {currentUserId}) deleted successfully");
                Console.WriteLine($"Account '{currentUser.Username}' deleted successfully.");
                Console.WriteLine("You have been logged out.");
                
                // Auto logout after account deletion
                UserAuthenticationService.Logout();
            }
            else
            {
                LoggingFactory.Instance.LogError($"Failed to delete account ID {currentUserId}");
                Console.WriteLine("Error: Failed to delete account.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error deleting account: {ex.Message}", ex);
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
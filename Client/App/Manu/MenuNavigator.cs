using Client.Database.UserAuthentication;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.App.Manu;

/// <summary>
/// Menu navigation handler for interactive user interface
/// </summary>
internal static class MenuNavigator
{
    private const int LogoutDelayMs = 1500;

    /// <summary>
    /// Navigate through the main menu interactively
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task NavigateMainMenuAsync()
    {
        try
        {
            while (true)
            {
                // Display main menu
                MainMenuHandler.DisplayMainMenu();

                var choice = MainMenuHandler.GetUserMenuChoice();

                // Handle menu choices based on login status
                if (UserAuthenticationService.CurrentUserStatus == LoginStatus.NotLoggedIn)
                {
                    // Not logged in - handle options 1, 2, 3
                    switch (choice)
                    {
                        case "1":
                            await MainMenuHandler.HandleUserLoginAsync();
                            break;
                        case "2":
                            await MainMenuHandler.HandleUserRegistrationAsync();
                            break;
                        case "3":
                            LoggingFactory.Instance.LogInformation("Thank you for using our application. Goodbye!");
                            return; // Exit application
                        default:
                            LoggingFactory.Instance.LogWarning("Invalid choice. Please select a valid option.");
                            Console.WriteLine("Press Enter to continue...");
                            Console.ReadLine();
                            break;
                    }
                }
                else
                {
                    // Logged in - handle menu with account management for all users
                    switch (choice)
                    {
                        case "1":
                            // Account Management - available to all logged-in users
                            await AccountManagementMenuHandler.HandleAccountManagementMenuAsync();
                            break;
                        case "2":
                            // Handle based on permission level
                            if (UserAuthenticationService.IsUserAdministrator())
                            {
                                // Administrator menu - User Management
                                await UserManagementMenuHandler.HandleUserManagementMenuAsync();
                            }
                            else
                            {
                                // Regular user menu - Logout
                                UserAuthenticationService.Logout();
                                LoggingFactory.Instance.LogInformation("You have been logged out successfully.");
                                await Task.Delay(LogoutDelayMs);
                            }
                            break;
                        case "3":
                            // Handle based on permission level
                            if (UserAuthenticationService.IsUserAdministrator())
                            {
                                // Administrator menu - Logout
                                UserAuthenticationService.Logout();
                                LoggingFactory.Instance.LogInformation("You have been logged out successfully.");
                                await Task.Delay(LogoutDelayMs);
                            }
                            else
                            {
                                // Regular user menu - Exit
                                LoggingFactory.Instance.LogInformation("Thank you for using our application. Goodbye!");
                                return; // Exit application
                            }
                            break;
                        case "4":
                            // Only administrators have 4 options
                            if (!UserAuthenticationService.IsUserAdministrator())
                            {
                                // Invalid choice for regular users
                                goto default;
                            }
                            
                            LoggingFactory.Instance.LogInformation("Thank you for using our application. Goodbye!");
                            return; // Exit application
                        default:
                            LoggingFactory.Instance.LogWarning("Invalid choice. Please select a valid option.");
                            Console.WriteLine("Press Enter to continue...");
                            Console.ReadLine();
                            break;
                    }
                }
                // Auto continue to next iteration
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Error in main menu navigation: {ex.Message}", ex);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
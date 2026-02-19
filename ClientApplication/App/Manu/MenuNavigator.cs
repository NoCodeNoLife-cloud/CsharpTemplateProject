using LoggingService.Services;

namespace ClientApplication.App.Manu;

/// <summary>
/// Menu navigation handler for interactive user interface
/// </summary>
internal static class MenuNavigator
{
    /// <summary>
    /// Navigate through the main menu interactively
    /// </summary>
    public static async Task NavigateMainMenuAsync()
    {
        try
        {
            while (true)
            {
                // Display main menu
                MainMenuHandler.DisplayMainMenu();

                var choice = MainMenuHandler.GetUserMenuChoice();

                switch (choice)
                {
                    case "1":
                        await MainMenuHandler.HandleUserLoginAsync();
                        break;
                    case "2":
                        await MainMenuHandler.HandleUserRegistrationAsync();
                        break;
                    case "3":
                        await UserManagementMenuHandler.HandleUserManagementMenuAsync();
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
            LoggingServiceImpl.InstanceVal.LogError($"Unexpected error during menu navigation: {ex.Message}", ex);
            LoggingServiceImpl.InstanceVal.LogDebug("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
using LoggingService.Services;

namespace ClientApplication.Database.UserAuthentication;

/// <summary>
/// Validates user login credentials and prevents duplicate entries
/// </summary>
public static class UserLoginValidator
{
    /// <summary>
    /// Validates user authentication by testing login with different credentials
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task<bool> ValidateLoginCredentialsAsync()
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogInformation("=== Starting Login Authentication Validation ===");

            // First, ensure database is set up
            var setupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
            if (!setupSuccess)
            {
                LoggingServiceImpl.InstanceVal.LogError("Database setup failed");
                return false;
            }

            // Test user data
            const string testUsername = "test_user";
            const string testPassword = "test_password";
            const string wrongPassword = "wrong_password";

            // Validation 1: Test authentication with non-existent user
            LoggingServiceImpl.InstanceVal.LogDebug("Validation 1: Testing authentication with non-existent user");
            var (authResult1, _, _) = await UserAuthenticationService.AuthenticateUserAsync("nonexistent_user", "any_password");
            LoggingServiceImpl.InstanceVal.LogInformation($"Non-existent user authentication result: {authResult1}");

            // Validation 2: Test authentication with correct credentials
            LoggingServiceImpl.InstanceVal.LogDebug("Validation 2: Testing authentication with correct credentials");
            var (authResult2, _, username2) = await UserAuthenticationService.AuthenticateUserAsync(testUsername, testPassword);
            LoggingServiceImpl.InstanceVal.LogInformation($"Correct credentials authentication result: {authResult2}, User: {username2}");

            // Validation 3: Test authentication with wrong password
            LoggingServiceImpl.InstanceVal.LogDebug("Validation 3: Testing authentication with wrong password");
            var (authResult3, _, _) = await UserAuthenticationService.AuthenticateUserAsync(testUsername, wrongPassword);
            LoggingServiceImpl.InstanceVal.LogInformation($"Wrong password authentication result: {authResult3}");

            LoggingServiceImpl.InstanceVal.LogInformation("=== Login Authentication Validation Complete ===");

            // Success criteria: non-existent user should fail, correct credentials should succeed, wrong password should fail
            return !authResult1 && authResult2 && !authResult3;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Login authentication validation failed: {ex.Message}");
            return false;
        }
    }
}
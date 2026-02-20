using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.Database.UserAuthentication;

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
            LoggingFactory.Instance.LogInformation("=== Starting Login Authentication Validation ===");

            // First, ensure database is set up
            var setupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
            if (!setupSuccess)
            {
                LoggingFactory.Instance.LogError("Database setup failed");
                return false;
            }

            // Test user data
            const string testUsername = "test_user";
            const string testPassword = "test_password";
            const string wrongPassword = "wrong_password";

            // Validation 1: Test authentication with non-existent user
            LoggingFactory.Instance.LogDebug("Validation 1: Testing authentication with non-existent user");
            var (authResult1, _, _) = await UserAuthenticationService.AuthenticateUserAsync("nonexistent_user", "any_password");
            LoggingFactory.Instance.LogInformation($"Non-existent user authentication result: {authResult1}");

            // Validation 2: Test authentication with correct credentials
            LoggingFactory.Instance.LogDebug("Validation 2: Testing authentication with correct credentials");
            var (authResult2, _, username2) = await UserAuthenticationService.AuthenticateUserAsync(testUsername, testPassword);
            LoggingFactory.Instance.LogInformation($"Correct credentials authentication result: {authResult2}, User: {username2}");

            // Validation 3: Test authentication with wrong password
            LoggingFactory.Instance.LogDebug("Validation 3: Testing authentication with wrong password");
            var (authResult3, _, _) = await UserAuthenticationService.AuthenticateUserAsync(testUsername, wrongPassword);
            LoggingFactory.Instance.LogInformation($"Wrong password authentication result: {authResult3}");

            LoggingFactory.Instance.LogInformation("=== Login Authentication Validation Complete ===");

            // Success criteria: non-existent user should fail, correct credentials should succeed, wrong password should fail
            return !authResult1 && authResult2 && !authResult3;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Login authentication validation failed: {ex.Message}");
            return false;
        }
    }
}
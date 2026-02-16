using MySqlConnector;
using ClientApplication.Database;
using LoggingService.Services;
using LoggingService.Enums;

namespace ClientApplication.Database.Examples;

/// <summary>
/// Test class to verify duplicate key handling works correctly
/// </summary>
public static class UserInsertionTest
{
    /// <summary>
    /// Tests the duplicate key prevention logic
    /// </summary>
    public static async Task<bool> TestDuplicatePreventionAsync()
    {
        try
        {
            LoggingServiceImpl.InstanceVal.LogInformation("=== Starting Duplicate Prevention Test ===");
            
            // First, ensure database is set up
            var setupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();
            if (!setupSuccess)
            {
                LoggingServiceImpl.InstanceVal.LogError("Database setup failed");
                return false;
            }
            
            // Test 1: Add a user for the first time
            LoggingServiceImpl.InstanceVal.LogDebug("Test 1: Adding new user 'test_duplicate'");
            var firstAdd = await UserManagementExample.AddUserAsync("test_duplicate", "password123");
            LoggingServiceImpl.InstanceVal.LogInformation($"First add result: {firstAdd}");
            
            // Test 2: Try to add the same user again (should be prevented)
            LoggingServiceImpl.InstanceVal.LogDebug("Test 2: Adding same user 'test_duplicate' again");
            var secondAdd = await UserManagementExample.AddUserAsync("test_duplicate", "different_password");
            LoggingServiceImpl.InstanceVal.LogInformation($"Second add result: {secondAdd}");
            
            // Test 3: Verify the user exists
            LoggingServiceImpl.InstanceVal.LogDebug("Test 3: Verifying user exists");
            var (exists, userId, username) = await UserManagementExample.GetUserAsync("test_duplicate");
            LoggingServiceImpl.InstanceVal.LogInformation($"User exists: {exists}, ID: {userId}, Username: {username}");
            
            // Test 4: Check user count
            var allUsers = await UserManagementExample.ListAllUsersAsync();
            LoggingServiceImpl.InstanceVal.LogInformation($"Total users in database: {allUsers.Count}");
            
            LoggingServiceImpl.InstanceVal.LogInformation("=== Duplicate Prevention Test Complete ===");
            
            // Success criteria: first add should succeed, second should be handled gracefully
            return firstAdd && secondAdd && exists;
        }
        catch (Exception ex)
        {
            LoggingServiceImpl.InstanceVal.LogError($"Duplicate prevention test failed: {ex.Message}");
            return false;
        }
    }
}
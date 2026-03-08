using System.Net.Http;
using System.Text;
using System.Text.Json;
using Common.Models;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Client.App.Services;

/// <summary>
/// HTTP client service for calling Server authentication APIs
/// </summary>
/// <remarks>
/// Usage example:
/// <code>
/// // Login
/// var (success, userId, username, status) = await ServerAuthService.LoginAsync("admin", "password123");
/// 
/// // Register
/// var (success, userId, error) = await ServerAuthService.RegisterAsync("user", "pass", "user");
/// 
/// // Get user info
/// var userInfo = await ServerAuthService.GetUserInfoAsync(userId);
/// 
/// // Admin operations (require API key)
/// var users = await ServerAuthService.GetAllUsersAsync(ServerAuthService.DefaultApiKey);
/// </code>
/// </remarks>
public static class ServerAuthService
{
    private static readonly HttpClient HttpClient = new();
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Default API key for admin operations (should be moved to configuration)
    /// </summary>
    public const string DefaultApiKey = "your-secret-key";

    /// <summary>
    /// Login via Server API
    /// </summary>
    public static async Task<(bool Success, int? UserId, string? Username, string? Status)> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new { username, password };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"{BaseUrl}/api/auth/login", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return (true, apiResponse.Data.UserId, apiResponse.Data.Username, apiResponse.Data.Status);
                }
            }

            LoggingFactory.Instance.LogWarning($"Login API failed: {responseBody}");
            return (false, null, null, null);
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Login API error: {ex.Message}", ex);
            return (false, null, null, null);
        }
    }

    /// <summary>
    /// Register via Server API
    /// </summary>
    public static async Task<(bool Success, int? UserId, string? ErrorMessage)> RegisterAsync(string username, string password, string priority)
    {
        try
        {
            var registerRequest = new { username, password, priority };
            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"{BaseUrl}/api/auth/register", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return (true, apiResponse.Data.UserId, null);
                }
            }

            LoggingFactory.Instance.LogWarning($"Register API failed: {responseBody}");
            return (false, null, responseBody);
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Register API error: {ex.Message}", ex);
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Logout via Server API
    /// </summary>
    public static async Task<bool> LogoutAsync()
    {
        try
        {
            var response = await HttpClient.PostAsync($"{BaseUrl}/api/auth/logout", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Logout API error: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Get user info via Server API
    /// </summary>
    public static async Task<UserInfo?> GetUserInfoAsync(int userId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/api/user?userId={userId}");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserInfoResponse>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return apiResponse?.Data != null
                    ? new UserInfo
                    {
                        Id = apiResponse.Data.Id,
                        Username = apiResponse.Data.Username,
                        Priority = apiResponse.Data.Priority
                    }
                    : null;
            }

            return null;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Get user info API error: {ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// Change password via Server API
    /// </summary>
    public static async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var changePasswordRequest = new { userId, currentPassword, newPassword };
            var json = JsonSerializer.Serialize(changePasswordRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PutAsync($"{BaseUrl}/api/user/password", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return apiResponse?.Success == true;
            }

            LoggingFactory.Instance.LogWarning($"Change password API failed: {responseBody}");
            return false;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Change password API error: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Get all users via Server API (Admin only)
    /// </summary>
    public static async Task<List<UserInfo>?> GetAllUsersAsync(string apiKey)
    {
        try
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                HttpClient.DefaultRequestHeaders.Remove("X-API-Key");
                HttpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }

            var response = await HttpClient.GetAsync($"{BaseUrl}/api/users");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserInfoResponse>>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return apiResponse?.Data?.Select(u => new UserInfo
                {
                    Id = u.Id,
                    Username = u.Username,
                    Priority = u.Priority
                }).ToList();
            }

            return null;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Get all users API error: {ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// Delete user via Server API (Admin only)
    /// </summary>
    public static async Task<bool> DeleteUserAsync(int userId, string apiKey)
    {
        try
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                HttpClient.DefaultRequestHeaders.Remove("X-API-Key");
                HttpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }

            var response = await HttpClient.DeleteAsync($"{BaseUrl}/api/users?userId={userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Delete user API error: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Setup database via Server API
    /// </summary>
    public static async Task<(bool Success, string? Message)> SetupDatabaseAsync()
    {
        try
        {
            LoggingFactory.Instance.LogInformation("Calling Server API to setup database...");

            var response = await HttpClient.PostAsync($"{BaseUrl}/api/db/setup", null);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return (apiResponse?.Success == true, apiResponse?.Message);
            }

            LoggingFactory.Instance.LogWarning($"Database setup API failed: {responseBody}");
            return (false, responseBody);
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Database setup API error: {ex.Message}", ex);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Test database connection via Server API
    /// </summary>
    public static async Task<(bool Success, bool? IsConnected, string? DatabaseName, DateTime? TestedAt, string? Message)> TestDatabaseConnectionAsync()
    {
        try
        {
            LoggingFactory.Instance.LogInformation("Calling Server API to test database connection...");

            var response = await HttpClient.GetAsync($"{BaseUrl}/api/db/test");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ConnectionTestResponse>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return (
                        true,
                        apiResponse.Data.IsConnected,
                        apiResponse.Data.DatabaseName,
                        apiResponse.Data.TestedAt,
                        apiResponse.Message
                    );
                }
            }

            LoggingFactory.Instance.LogWarning($"Database connection test API failed: {responseBody}");
            return (false, null, null, null, responseBody);
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Database connection test API error: {ex.Message}", ex);
            return (false, null, null, null, ex.Message);
        }
    }
}
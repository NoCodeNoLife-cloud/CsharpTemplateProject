using System.Text.Json;
using Common.Models;
using Microsoft.AspNetCore.Http;

namespace Server.Database.UserAuthentication;

/// <summary>
/// User authentication API service for WebHost
/// </summary>
public static class UserAuthApiService
{
    /// <summary>
    /// Handle login request
    /// </summary>
    public static async Task HandleLoginAsync(HttpContext context)
    {
        try
        {
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var loginRequest = JsonSerializer.Deserialize<LoginRequest>(requestBody);
            
            if (string.IsNullOrEmpty(loginRequest?.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                await WriteJsonResponse(context, 400, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "Username and password are required" 
                });
                return;
            }
            
            var (success, userId, username) = await UserAuthenticationService.AuthenticateUserAsync(
                loginRequest.Username, 
                loginRequest.Password
            );
            
            if (success)
            {
                var status = UserAuthenticationService.CurrentUserStatus.ToString();
                await WriteJsonResponse(context, 200, new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new LoginResponse
                    {
                        Success = true,
                        UserId = userId,
                        Username = username,
                        Status = status
                    }
                });
            }
            else
            {
                await WriteJsonResponse(context, 401, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }
        }
        catch (Exception ex)
        {
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Login failed: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Handle register request
    /// </summary>
    public static async Task HandleRegisterAsync(HttpContext context)
    {
        try
        {
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var registerRequest = JsonSerializer.Deserialize<RegisterRequest>(requestBody);
            
            if (string.IsNullOrEmpty(registerRequest?.Username) || string.IsNullOrEmpty(registerRequest.Password))
            {
                await WriteJsonResponse(context, 400, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Username and password are required"
                });
                return;
            }
            
            var (success, userId, errorMessage) = await UserAuthenticationService.RegisterUserAsync(
                registerRequest.Username,
                registerRequest.Password,
                registerRequest.Priority ?? "user"
            );
            
            if (success)
            {
                await WriteJsonResponse(context, 201, new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = new LoginResponse
                    {
                        Success = true,
                        UserId = userId,
                        Username = registerRequest.Username
                    }
                });
            }
            else
            {
                await WriteJsonResponse(context, 400, new ApiResponse<object>
                {
                    Success = false,
                    Message = errorMessage ?? "Registration failed"
                });
            }
        }
        catch (Exception ex)
        {
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Registration failed: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Handle logout request
    /// </summary>
    public static async Task HandleLogoutAsync(HttpContext context)
    {
        try
        {
            UserAuthenticationService.Logout();
            await WriteJsonResponse(context, 200, new ApiResponse<object>
            {
                Success = true,
                Message = "Logout successful"
            });
        }
        catch (Exception ex)
        {
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Logout failed: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Handle get user info request
    /// </summary>
    public static async Task HandleGetUserInfoAsync(HttpContext context)
    {
        try
        {
            var userIdParam = context.Request.Query["userId"].ToString();
            
            if (!int.TryParse(userIdParam, out var userId))
            {
                await WriteJsonResponse(context, 400, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
                return;
            }
            
            var user = await UserAuthenticationService.GetUserByIdAsync(userId);
            
            if (user != null)
            {
                await WriteJsonResponse(context, 200, new ApiResponse<UserInfoResponse>
                {
                    Success = true,
                    Message = "User found",
                    Data = new UserInfoResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Priority = user.Priority
                    }
                });
            }
            else
            {
                await WriteJsonResponse(context, 404, new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found"
                });
            }
        }
        catch (Exception ex)
        {
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Failed to get user: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Handle change password request
    /// </summary>
    public static async Task HandleChangePasswordAsync(HttpContext context)
    {
        try
        {
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var changePasswordRequest = JsonSerializer.Deserialize<ChangePasswordRequest>(requestBody);
            
            if (changePasswordRequest == null || changePasswordRequest.UserId <= 0 ||
                string.IsNullOrEmpty(changePasswordRequest.CurrentPassword) ||
                string.IsNullOrEmpty(changePasswordRequest.NewPassword))
            {
                await WriteJsonResponse(context, 400, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request parameters"
                });
                return;
            }
            
            // Verify current password
            var (authenticated, _, _) = await UserAuthenticationService.AuthenticateUserAsync(
                changePasswordRequest.UserId.ToString(),
                changePasswordRequest.CurrentPassword
            );
            
            if (!authenticated)
            {
                await WriteJsonResponse(context, 401, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Current password is incorrect"
                });
                return;
            }
            
            // Update password
            var success = await UserAuthenticationService.UpdateUserPasswordAsync(
                changePasswordRequest.UserId,
                changePasswordRequest.NewPassword
            );
            
            if (success)
            {
                await WriteJsonResponse(context, 200, new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password changed successfully"
                });
            }
            else
            {
                await WriteJsonResponse(context, 500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to change password"
                });
            }
        }
        catch (Exception ex)
        {
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Failed to change password: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Handle get all users request (admin only)
    /// </summary>
    public static async Task HandleGetAllUsersAsync(HttpContext context)
    {
        try
        {
            var users = await UserAuthenticationService.GetAllUsersAsync();
            var userList = users.Select(u => new UserInfoResponse
            {
                Id = u.Id,
                Username = u.Username,
                Priority = u.Priority
            }).ToList();
            
            await WriteJsonResponse(context, 200, new ApiResponse<List<UserInfoResponse>>
            {
                Success = true,
                Message = $"Retrieved {userList.Count} users",
                Data = userList
            });
        }
        catch (Exception ex)
        {
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Failed to get users: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Handle delete user request (admin only)
    /// </summary>
    public static async Task HandleDeleteUserAsync(HttpContext context)
    {
        try
        {
            var userIdParam = context.Request.Query["userId"].ToString();
            
            if (!int.TryParse(userIdParam, out var userId))
            {
                await WriteJsonResponse(context, 400, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid user ID"
                });
                return;
            }
            
            var success = await UserAuthenticationService.DeleteUserAsync(userId);
            
            if (success)
            {
                await WriteJsonResponse(context, 200, new ApiResponse<object>
                {
                    Success = true,
                    Message = "User deleted successfully"
                });
            }
            else
            {
                await WriteJsonResponse(context, 404, new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found or deletion failed"
                });
            }
        }
        catch (Exception ex)
        {
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Failed to delete user: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Write JSON response
    /// </summary>
    private static async Task WriteJsonResponse<T>(HttpContext context, int statusCode, ApiResponse<T> response)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }
}

using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Common.Models.Responses;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace Server.Database;

/// <summary>
/// Database management API service for handling database setup and testing operations
/// </summary>
public static class DatabaseManagementApiService
{
    /// <summary>
    /// Handle database setup request
    /// </summary>
    public static async Task HandleSetupDatabaseAsync(HttpContext context)
    {
        try
        {
            LoggingFactory.Instance.LogInformation("Received database setup request");

            // Perform database setup
            var setupSuccess = await DatabaseSetupUtility.SetupDemoDatabaseAsync();

            if (setupSuccess)
            {
                await WriteJsonResponse(context, 200, new ApiResponse<object>
                {
                    Success = true,
                    Message = "Database setup completed successfully"
                });
            }
            else
            {
                await WriteJsonResponse(context, 500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Database setup failed. Check server logs for details."
                });
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Database setup API error: {ex.Message}", ex);
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Database setup failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Handle database connection test request
    /// </summary>
    public static async Task HandleTestDatabaseConnectionAsync(HttpContext context)
    {
        try
        {
            LoggingFactory.Instance.LogInformation("Received database connection test request");

            // Test database connection
            var connectionSuccess = await DatabaseSetupUtility.TestDemoDatabaseConnectionAsync();

            if (connectionSuccess)
            {
                await WriteJsonResponse(context, 200, new ApiResponse<ConnectionTestResponse>
                {
                    Success = true,
                    Message = "Database connection test successful",
                    Data = new ConnectionTestResponse
                    {
                        IsConnected = true,
                        DatabaseName = "demo",
                        TestedAt = DateTime.UtcNow
                    }
                });
            }
            else
            {
                await WriteJsonResponse(context, 500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Database connection test failed"
                });
            }
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError($"Database connection test API error: {ex.Message}", ex);
            await WriteJsonResponse(context, 500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Database connection test failed: {ex.Message}"
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
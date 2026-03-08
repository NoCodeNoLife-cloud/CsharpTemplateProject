using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Server.App;

/// <summary>
/// Dynamic endpoint registration service
/// </summary>
public static class EndpointRegistrar
{
    /// <summary>
    /// Register single endpoint
    /// </summary>
    public static void RegisterEndpoint(IApplicationBuilder app, EndpointConfig config)
    {
        app.Map(config.Path, builder =>
        {
            builder.Use(async (context, next) =>
            {
                // Check HTTP method
                if (!string.Equals(context.Request.Method, config.Method, StringComparison.OrdinalIgnoreCase))
                {
                    await next();
                    return;
                }
                
                // Check authentication
                if (config.RequireAuth)
                {
                    // TODO: Add your authentication logic here
                    if (!await AuthenticateAsync(context))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                }
                
                // Execute custom handler or default response
                if (config.Handler != null)
                {
                    await config.Handler(context);
                }
                else
                {
                    context.Response.ContentType = config.ContentType;
                    await context.Response.WriteAsync(config.Response);
                }
            });
        });
    }
    
    /// <summary>
    /// Batch register endpoints from configuration list
    /// </summary>
    public static void RegisterEndpoints(IApplicationBuilder app, IEnumerable<EndpointConfig> configs)
    {
        foreach (var config in configs)
        {
            RegisterEndpoint(app, config);
        }
    }
    
    /// <summary>
    /// Simple authentication check (replace with real logic)
    /// </summary>
    private static async Task<bool> AuthenticateAsync(HttpContext context)
    {
        // Check for API key in header
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        return !string.IsNullOrEmpty(apiKey) && apiKey == "your-secret-key";
    }
}
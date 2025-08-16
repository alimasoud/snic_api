using snic_api.Services;
using System.Net;

namespace snic_api.Middleware
{
    public class JwtBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TokenBlacklistService tokenBlacklistService)
        {
            var token = ExtractTokenFromHeader(context);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var isBlacklisted = await tokenBlacklistService.IsTokenBlacklistedAsync(token);
                    if (isBlacklisted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsync("Token has been invalidated");
                        return;
                    }
                }
                catch
                {
                    // If there's an error checking the blacklist, continue with the request
                    // This ensures the system remains functional even if blacklist checking fails
                }
            }

            await _next(context);
        }

        private string? ExtractTokenFromHeader(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }
    }

    public static class JwtBlacklistMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtBlacklist(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtBlacklistMiddleware>();
        }
    }
} 
using Microsoft.AspNetCore.Http.HttpResults;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Models.Responses;
using SupportChat.Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;
namespace SupportChat.Backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth")
            .WithTags("Authentication")
            .AllowAnonymous(); // Login/Refresh are public

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithDescription("Authenticate user and return JWT tokens.");

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithDescription("Refresh access token using refresh token.");

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithDescription("Revoke refresh token and logout.")
            .RequireAuthorization(); // Requires valid token

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithDescription("Register a new user (agent/admin).")
            .RequireAuthorization("Admin"); // Only admins can create users (or system)
    }

    private static async Task<IResult> LoginAsync(LoginRequest request,IAuthService authService,ILogger<Program> logger)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Login failed: {Message}", ex.Message);
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login.");
            return Results.Problem("An error occurred during login.");
        }
    }

    private static async Task<IResult> RefreshTokenAsync(RefreshTokenRequest request,IAuthService authService,ILogger<Program> logger)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Refresh token failed: {Message}", ex.Message);
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during refresh token.");
            return Results.Problem("An error occurred during token refresh.");
        }
    }

    private static async Task<IResult> LogoutAsync(HttpContext httpContext,IAuthService authService,ILogger<Program> logger)
    {
        try
        {
            // Get refresh token from request body or header
            var refreshToken = httpContext.Request.Headers["Refresh-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(refreshToken))
                return Results.BadRequest("Refresh token is required.");

            await authService.LogoutAsync(refreshToken);
            return Results.Ok(new { message = "Logged out successfully." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout.");
            return Results.Problem("An error occurred during logout.");
        }
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request,IAuthService authService,ILogger<Program> logger)
    {
        try
        {
            var userId = await authService.RegisterUserAsync(request);
            return Results.Ok(new { UserId = userId, Message = "User registered successfully." });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Registration failed: {Message}", ex.Message);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during registration.");
            return Results.Problem("An error occurred during registration.");
        }
    }
}
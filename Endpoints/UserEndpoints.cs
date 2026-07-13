using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Services.Interfaces;
using System.Security.Claims;

namespace SupportChat.Backend.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users").WithTags("Users").AllowAnonymous();//.RequireAuthorization("Admin");

        group.MapGet("/", GetUsersByCompanyAsync)
            .WithName("GetUsersByCompany")
            .WithDescription("Get all users (agents) for a company.");

        group.MapGet("/{id}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithDescription("Get a user by ID.");

        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .AllowAnonymous()
            .WithDescription("Get current logged in user.");

        group.MapPost("/", CreateUserAsync)
            .WithName("CreateUser")
            .WithDescription("Create a new user/agent.");

        group.MapPut("/{id}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithDescription("Update a user.");

        group.MapDelete("/{id}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithDescription("Soft delete a user.");
    }

    private static async Task<IResult> GetUsersByCompanyAsync([AsParameters] GetUsersRequest request,IUserService userService,HttpContext httpContext)
    {
        // We'll get companyId from the authenticated user or from query
        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid))
            return Results.BadRequest("Company not found.");

        var users = await userService.GetUsersByCompanyAsync(cid);
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserByIdAsync(long id, IUserService userService)
    {
        var user = await userService.GetByIdAsync(id);
        if (user == null)
            return Results.NotFound($"User with ID {id} not found.");
        return Results.Ok(user);
    }

    private static async Task<IResult> GetCurrentUserAsync(IUserService userService, HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var user = await userService.GetCurrentUserAsync(userId);
        if (user == null)
            return Results.NotFound($"User with ID {userId} not found.");

        return Results.Ok(user);
    }

    private static async Task<IResult> CreateUserAsync(CreateUserRequest request,ILogger<Program> logger,IUserService userService)
    {
        try
        {
            var userId = await userService.CreateUserAsync(request);
            // Use CreatedAtRoute so the base path / API version is resolved dynamically
            return Results.CreatedAtRoute("GetUserById", new { id = userId }, new { UserId = userId });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user.");
            return Results.Problem("An error occurred while creating the user.");
        }
    }

    private static async Task<IResult> UpdateUserAsync(long id,UpdateUserRequest request,IUserService userService,ILogger<Program> logger)
    {
        if (id != request.UserId)
            return Results.BadRequest("ID mismatch.");

        try
        {
            await userService.UpdateUserAsync(request);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user.");
            return Results.Problem("An error occurred while updating the user.");
        }
    }

    private static async Task<IResult> DeleteUserAsync(long id,IUserService userService,ILogger<Program> logger,HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var deletedBy))
            return Results.Unauthorized();

        try
        {
            await userService.DeleteUserAsync(id, deletedBy);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user.");
            return Results.Problem("An error occurred while deleting the user.");
        }
    }
}

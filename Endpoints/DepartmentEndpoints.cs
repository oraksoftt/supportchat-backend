using Microsoft.AspNetCore.Http.HttpResults;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Services.Interfaces;
using System.Security.Claims;

namespace SupportChat.Backend.Endpoints;

public static class DepartmentEndpoints
{
    public static void MapDepartmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/departments")
            .WithTags("Departments")
          //.AllowAnonymous();
        .RequireAuthorization("Admin");

        group.MapGet("/company", GetDepartmentsForCompanyAsync)
            .WithName("GetDepartments")
            .WithDescription("Get all departments for the company.");

        group.MapGet("/{id}", GetDepartmentByIdAsync)
            .WithName("GetDepartmentById")
            .WithDescription("Get a department by ID.");

        group.MapPost("/", CreateDepartmentAsync)
            .WithName("CreateDepartment")
            .WithDescription("Create a new department.");

        group.MapPut("/{id}", UpdateDepartmentAsync)
            .WithName("UpdateDepartment")
            .WithDescription("Update a department.");

        group.MapDelete("/{id}", DeleteDepartmentAsync)
            .WithName("DeleteDepartment")
            .WithDescription("Soft delete a department.");
    }

    private static async Task<IResult> GetDepartmentsForCompanyAsync(HttpContext httpContext, IDepartmentService departmentService)
    {
        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid))
            return Results.BadRequest("Company not found.");

        var departments = await departmentService.GetByCompanyAsync(cid);
        return Results.Ok(departments);
    }

    private static async Task<IResult> GetDepartmentByIdAsync(long id, IDepartmentService departmentService, HttpContext httpContext)
    {
        var dept = await departmentService.GetByIdAsync(id);
        if (dept == null)
            return Results.NotFound($"Department with ID {id} not found.");

        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid) || dept.CompanyId != cid)
            return Results.Forbid();

        return Results.Ok(dept);
    }

    private static async Task<IResult> CreateDepartmentAsync(CreateDepartmentRequest request, IDepartmentService departmentService, ILogger<Program> logger, HttpContext httpContext)
    {
        try
        {
            // Caller identity
            var callerCompanyClaim = httpContext.User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(callerCompanyClaim) || !long.TryParse(callerCompanyClaim, out var callerCompanyId))
                return Results.Unauthorized();

            // Allow only creating departments for the caller's company, unless SystemAdmin role
            if (request.CompanyId != callerCompanyId && !httpContext.User.IsInRole("Super Admin"))
                return Results.Forbid();

            // Set CreatedBy from caller if available
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
                request.CreatedBy = userId;

            var id = await departmentService.CreateAsync(request);
            return Results.CreatedAtRoute("GetDepartmentById", new { id }, new { DepartmentId = id });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating department.");
            return Results.Problem("An error occurred while creating department.");
        }
    }

    private static async Task<IResult> UpdateDepartmentAsync(long id, UpdateDepartmentRequest request, IDepartmentService departmentService, ILogger<Program> logger, HttpContext httpContext)
    {
        if (id != request.DepartmentId)
            return Results.BadRequest("ID mismatch.");

        try
        {
            // Ensure department exists and belongs to caller's company (or caller is system admin)
            var existing = await departmentService.GetByIdAsync(request.DepartmentId);
            if (existing == null)
                return Results.NotFound($"Department with ID {request.DepartmentId} not found.");

            var callerCompanyClaim = httpContext.User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(callerCompanyClaim) || !long.TryParse(callerCompanyClaim, out var callerCompanyId))
                return Results.Unauthorized();

            if (existing.CompanyId != callerCompanyId && !httpContext.User.IsInRole("SystemAdmin"))
                return Results.Forbid();

            // Set ModifiedBy from caller if available
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
                request.ModifiedBy = userId;

            await departmentService.UpdateAsync(request);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating department.");
            return Results.Problem("An error occurred while updating department.");
        }
    }

    private static async Task<IResult> DeleteDepartmentAsync(long id, IDepartmentService departmentService, ILogger<Program> logger, HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        try
        {
            // Ensure department exists and caller is authorized for that company
            var dept = await departmentService.GetByIdAsync(id);
            if (dept == null)
                return Results.NotFound($"Department with ID {id} not found.");

            var callerCompanyClaim = httpContext.User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(callerCompanyClaim) || !long.TryParse(callerCompanyClaim, out var callerCompanyId))
                return Results.Unauthorized();

            if (dept.CompanyId != callerCompanyId && !httpContext.User.IsInRole("SystemAdmin"))
                return Results.Forbid();

            await departmentService.DeleteAsync(id, userId);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting department.");
            return Results.Problem("An error occurred while deleting department.");
        }
    }
}
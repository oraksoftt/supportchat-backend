using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Services.Interfaces;
using System.Security.Claims;

namespace SupportChat.Backend.Endpoints;

public static class CompanyEndpoints
{
    public static void MapCompanyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/companies")
            .WithTags("Company")
            //.AllowAnonymous();
            .RequireAuthorization("Admin");

        group.MapGet("/", GetAllCompaniesAsync)
            .WithName("GetAllCompanies")
            .WithDescription("Get all companies.");

        group.MapGet("/{id}", GetCompanyByIdAsync)
            .WithName("GetCompanyById")
            .WithDescription("Get a company by ID.");

        group.MapPost("/", CreateCompanyAsync)
            .WithName("CreateCompany")
            .WithDescription("Create a new company.")
            .AllowAnonymous();

        group.MapPut("/{id}", UpdateCompanyAsync)
            .WithName("UpdateCompany")
            .WithDescription("Update a company.");

        group.MapDelete("/{id}", DeleteCompanyAsync)
            .WithName("DeleteCompany")
            .WithDescription("Soft delete a company.");
    }

    private static async Task<IResult> GetAllCompaniesAsync(ICompanyService service)
    {
        var companies = await service.GetAllAsync();
        return Results.Ok(companies);
    }

    private static async Task<IResult> GetCompanyByIdAsync(long id, ICompanyService service)
    {
        var company = await service.GetByIdAsync(id);
        if (company == null)
            return Results.NotFound($"Company with ID {id} not found.");
        return Results.Ok(company);
    }

    private static async Task<IResult> CreateCompanyAsync(CreateCompanyRequest request,ICompanyService service,ILogger<Program> logger)
    {
        try
        {
            var id = await service.CreateAsync(request);
            //return Results.Created($"/api/v1/companies/{id}", new { CompanyId = id });
            return Results.CreatedAtRoute("GetCompanyById", new { id = id }, new { CompanyId = id });
        }
        catch (BadHttpRequestException ex)
        {
            // Validation-like error from service
            return Results.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Domain error -> bad request
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating company.");
            return Results.Problem("An error occurred while creating the company.");
        }
    }

    private static async Task<IResult> UpdateCompanyAsync(
        long id,
        UpdateCompanyRequest request,
        ICompanyService service,
        ILogger<Program> logger)
    {
        if (id != request.CompanyId)
            return Results.BadRequest("ID mismatch.");

        try
        {
            await service.UpdateAsync(request);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating company.");
            return Results.Problem("An error occurred while updating the company.");
        }
    }

    private static async Task<IResult> DeleteCompanyAsync(
        long id,
        ICompanyService service,
        ILogger<Program> logger,
        HttpContext httpContext)
    {
        // Get current user ID from claims (for audit)
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        try
        {
            await service.DeleteAsync(id, userId);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting company.");
            return Results.Problem("An error occurred while deleting the company.");
        }
    }
}
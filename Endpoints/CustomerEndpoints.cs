using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Services.Interfaces;
using System.Net.Http;

namespace SupportChat.Backend.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/customers").WithTags("Customers").RequireAuthorization("Agent");

        group.MapGet("/", GetCustomersForCompanyAsync)
            .WithName("GetCustomers")
            .WithDescription("Get all customers for the company.");
        group.MapPost("/create-or-get", CreateOrGetCustomerAsync)
            .WithName("CreateOrGetCustomer")
            .WithDescription("Creates a new customer or retrieves the existing one based on the request details.")
            //.Produces<long>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapGet("/{id}", GetCustomerByIdAsync)
            .WithName("GetCustomerById")
            .WithDescription("Get a customer by ID.");

        group.MapPut("/{id}", UpdateCustomerAsync)
            .WithName("UpdateCustomer")
            .WithDescription("Update customer details.");

        group.MapPost("/{id}/block", BlockCustomerAsync)
            .WithName("BlockCustomer")
            .WithDescription("Block a customer.");

        group.MapPost("/{id}/unblock", UnblockCustomerAsync)
            .WithName("UnblockCustomer")
            .WithDescription("Unblock a customer.");
    }
 
    private static async Task<IResult> CreateOrGetCustomerAsync(HttpContext httpContext, 
        [FromServices] IHttpClientFactory httpClientFactory, 
        [FromBody] CreateOrGetCustomerRequest request,ICustomerService customerService)
    {
        var tokenCompanyId = httpContext.User.FindFirst("CompanyId")?.Value;
        //if (string.IsNullOrEmpty(tokenCompanyId) || !long.TryParse(tokenCompanyId, out var companyId))
        //{
        //    return Results.BadRequest("Company not found in authentication context.");
        //}
        //request.CompanyId = companyId;

        // 1. Resolve Client IP Address (Safeguarded against Proxy forwards)
        string rawIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                       ?? httpContext.Connection.RemoteIpAddress?.ToString()
                       ?? "127.0.0.1";

        // Clean out multi-chain proxy entries if present (Takes the initial client IP)
        if (!string.IsNullOrEmpty(rawIp) && rawIp.Contains(","))
        {
            rawIp = rawIp.Split(',')[0].Trim();
        }

        string country = "Unknown";
        string city = "Unknown";

        // 2. Intercept Loopbacks (ip-api cannot lookup localhost IPs)
        if (rawIp == "127.0.0.1" || rawIp == "::1" || string.IsNullOrEmpty(rawIp))
        {
            country = "Localhost";
            city = "Localhost";
        }
        else
        {
            try
            {
                // Create HTTP Client instance and query the free geo endpoint
                var client = httpClientFactory.CreateClient();

                // Optional but recommended: Add a timeout so the request doesn't hang forever
                client.Timeout = TimeSpan.FromSeconds(3);
                var response = await client.GetFromJsonAsync<IpApiResponse>($"http://ip-api.com/json/{rawIp}");
                if (response != null && response.Status == "success")
                {
                    country = response.Country ?? "Unknown";
                    city = response.City ?? "Unknown";
                }
            }
            catch (Exception ex)
            {
                // Log execution faults internally (keeps service active if API rate-limits hit)
                Console.WriteLine($"IP Geolocation lookup failed: {ex.Message}");
            }
        }       
        request.IpAddress = rawIp;
        request.Country = country;
        request.City = city;
        
        var customerId = await customerService.CreateOrGetAsync(request.CompanyId, request);

        return Results.Ok(customerId);
    }
    private static async Task<IResult> GetCustomersForCompanyAsync(HttpContext httpContext,ICustomerService customerService)
    {
        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid))
            return Results.BadRequest("Company not found.");

        var customers = await customerService.GetByCompanyAsync(cid);
        return Results.Ok(customers);
    }

    private static async Task<IResult> GetCustomerByIdAsync(long id,ICustomerService customerService,HttpContext httpContext)
    {
        var customer = await customerService.GetByIdAsync(id);
        if (customer == null)
            return Results.NotFound($"Customer with ID {id} not found.");

        // Verify company ownership
        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid) || customer.CompanyId != cid)
            return Results.Forbid();

        return Results.Ok(customer);
    }

    private static async Task<IResult> UpdateCustomerAsync(long id,UpdateCustomerRequest request,ICustomerService customerService,ILogger<Program> logger)
    {
        if (id != request.CustomerId)
            return Results.BadRequest("ID mismatch.");

        try
        {
            await customerService.UpdateAsync(request);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating customer.");
            return Results.Problem("An error occurred while updating customer.");
        }
    }

    private static async Task<IResult> BlockCustomerAsync(long id,ICustomerService customerService,ILogger<Program> logger)
    {
        try
        {
            await customerService.BlockAsync(id);
            return Results.Ok(new { Message = "Customer blocked successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error blocking customer.");
            return Results.Problem("An error occurred while blocking customer.");
        }
    }

    private static async Task<IResult> UnblockCustomerAsync(long id,ICustomerService customerService,ILogger<Program> logger)
    {
        try
        {
            await customerService.UnblockAsync(id);
            return Results.Ok(new { Message = "Customer unblocked successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unblocking customer.");
            return Results.Problem("An error occurred while unblocking customer.");
        }
    }
}
public class IpApiResponse
{
    public string Status { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
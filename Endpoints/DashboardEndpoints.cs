using Microsoft.AspNetCore.Http.HttpResults;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/dashboard").WithTags("Dashboard").RequireAuthorization("Agent");

        group.MapGet("/stats", GetDashboardStatsAsync)
            .WithName("GetDashboardStats")
            .WithDescription("Get dashboard statistics for the company.");
    }

    private static async Task<IResult> GetDashboardStatsAsync(HttpContext httpContext,IDashboardService dashboardService)
    {
        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid))
            return Results.BadRequest("Company not found.");

        var stats = await dashboardService.GetDashboardStatsAsync(cid);
        return Results.Ok(stats);
    }
}
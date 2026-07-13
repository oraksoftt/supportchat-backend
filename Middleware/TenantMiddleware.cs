using System.Security.Claims;

namespace SupportChat.Backend.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get CompanyId from the authenticated user's claims
        var companyIdClaim = context.User.FindFirst("CompanyId");
        if (companyIdClaim != null && long.TryParse(companyIdClaim.Value, out long companyId))
        {
            context.Items["CompanyId"] = companyId;
        }

        await _next(context);
    }
}
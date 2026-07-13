using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SupportChat.Backend.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SupportChat.Backend.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;
    private readonly JwtSettings _jwtSettings;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger, IOptions<JwtSettings> jwtOptions)
    {
        _next = next;
        _logger = logger;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var authHeader = context.Request.Headers["Authorization"]
                .FirstOrDefault();


            if (string.IsNullOrWhiteSpace(authHeader))
            {
                await _next(context);
                return;
            }


            if (!authHeader.StartsWith("Bearer ",
                StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }


            var token = authHeader.Substring("Bearer ".Length)
                .Trim();


            // protect against empty/malformed token
            if (string.IsNullOrWhiteSpace(token))
            {
                await _next(context);
                return;
            }


            var handler = new JwtSecurityTokenHandler();


            if (!handler.CanReadToken(token))
            {
                await _next(context);
                return;
            }


            var jwtToken = handler.ReadJwtToken(token);


            // your existing logic here
            // claims extraction etc.


        }
        catch (SecurityTokenMalformedException ex)
        {
            _logger.LogWarning(
                ex,
                "Malformed JWT received"
            );
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(
                ex,
                "Expired JWT received"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "JWT middleware failed"
            );
        }


        await _next(context);
    }
}
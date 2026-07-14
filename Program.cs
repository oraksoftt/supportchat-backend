using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using SupportChat.Backend.Extensions;
using SupportChat.Backend.Helpers;
using SupportChat.Backend.Hubs;
using SupportChat.Backend.Middleware;
using SupportChat.Backend.Services;
using SupportChat.Backend.Services.Interfaces;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Serilog; // Added for logging

public partial class Program
{
    public static void Main(string[] args)
    {

        //  Serilog configuration
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/api-runtime-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "===================================================================\n[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}\n{Exception}===================================================================\n\n"
            )
            .CreateLogger();
        try
        {
            Log.Information("Starting SupportChat API host...");
            var builder = WebApplication.CreateBuilder(args);

            // 🛠️ Wire up Serilog to the application host
            builder.Host.UseSerilog();

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // Add services
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddRepositories();
            builder.Services.AddServices();
            builder.Services.AddSingleton<JwtHelper>();
            builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            // Authentication & Authorization
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var path = context.HttpContext.Request.Path;
                            if (path.StartsWithSegments("/chathub"))
                            {
                                var accessToken = context.Request.Query["access_token"];
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    context.Token = accessToken;
                                }
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin", "Super Admin", "Company Admin"));
                options.AddPolicy("Agent", policy => policy.RequireRole("Agent", "Admin", "Super Admin", "Company Admin"));
            });

            // SignalR
            builder.Services.AddSignalR();

            // 🛠️ API Versioning Setup
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>()
                .Configure<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>((options, provider) =>
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, new OpenApiInfo
                        {
                            Title = $"SupportChat Backend API {description.GroupName.ToUpperInvariant()}",
                            Version = description.ApiVersion.ToString()
                        });
                    }
                });

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Standard Logging registration is replaced by Serilog registration up top
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var descriptions = app.DescribeApiVersions();
                    foreach (var description in descriptions)
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = description.GroupName.ToUpperInvariant();
                        options.SwaggerEndpoint(url, $"SupportChat Backend API {name}");
                    }
                });
            }

            // Middleware pipeline
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<JwtMiddleware>();
            app.UseMiddleware<TenantMiddleware>();
            app.UseWebSockets();
            app.MapHub<ChatHub>("/chathub");
            app.MapEndpoints();

            //app.UseMiddleware<ErrorHandlingMiddleware>();
            //app.UseMiddleware<JwtMiddleware>();
            //app.UseMiddleware<TenantMiddleware>();

            //app.UseCors("AllowFrontend");

            //app.UseAuthentication();
            //app.UseAuthorization();

            //// Map Routes
            //app.MapHub<ChatHub>("/chathub");
            //app.MapEndpoints();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
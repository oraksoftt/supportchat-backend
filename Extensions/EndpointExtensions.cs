using SupportChat.Backend.Endpoints;

namespace SupportChat.Backend.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        // Version 1 endpoints
        //var apiGroup = app.MapGroup("/api/v2");
        //var apiGroup = app.MapGroup("/api/v{version:apiVersion}");

        var versionedApi = app.NewVersionedApi("SupportChat");
        var apiGroup = versionedApi.MapGroup("/api/v{version:apiVersion}");

        apiGroup.MapAuthEndpoints();
        apiGroup.MapCompanyEndpoints();
        apiGroup.MapUserEndpoints();
        apiGroup.MapChatEndpoints();
        apiGroup.MapMessageEndpoints();
        apiGroup.MapCustomerEndpoints();
        apiGroup.MapDepartmentEndpoints();
        apiGroup.MapNotificationEndpoints();
        apiGroup.MapDashboardEndpoints();

        return app;
    }
}
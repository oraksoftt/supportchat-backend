using SupportChat.Backend.Helpers;
using SupportChat.Backend.Repositories;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services;
using SupportChat.Backend.Services.Interfaces;


namespace SupportChat.Backend.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserService, UserService>(); 
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<JwtHelper>();
        return services;
    }
}
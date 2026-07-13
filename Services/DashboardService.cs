// DashboardService.cs
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class DashboardService : IDashboardService
{
    private readonly IChatRepository _chatRepo;
    private readonly IAuthRepository _authRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IChatRepository chatRepo,
        IAuthRepository authRepo,
        ICustomerRepository customerRepo,
        ILogger<DashboardService> logger)
    {
        _chatRepo = chatRepo;
        _authRepo = authRepo;
        _customerRepo = customerRepo;
        _logger = logger;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(long companyId)
    {
        // Get counts from repositories
        var totalChats = await _chatRepo.CountChatsByCompanyAsync(companyId);
        var activeChats = await _chatRepo.CountActiveChatsByCompanyAsync(companyId);
        var waitingChats = await _chatRepo.CountWaitingChatsByCompanyAsync(companyId);
        var closedChats = await _chatRepo.CountClosedChatsByCompanyAsync(companyId);

        // Fix: Count agents by getting users and filtering by agent role
        var users = await _authRepo.GetUsersByCompanyAsync(companyId);
        var totalAgents = users.Count(u => u.Role == "Agent");

        //var totalCustomers = await _customerRepo.CountCustomersByCompanyAsync(companyId);
        var customers = await _customerRepo.GetCustomersByCompanyAsync(companyId);
        var totalCustomers = customers.Count();

        // Average response time - we could calculate from chat assignments
        var avgResponseTime = await _chatRepo.GetAverageResponseTimeAsync(companyId);

        return new DashboardStats
        {
            TotalChats = totalChats,
            ActiveChats = activeChats,
            WaitingChats = waitingChats,
            ClosedChats = closedChats,
            TotalAgents = totalAgents,
            AverageResponseTimeMinutes = avgResponseTime,
            TotalCustomers = totalCustomers
        };
    }
}
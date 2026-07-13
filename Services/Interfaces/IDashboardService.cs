// IDashboardService.cs
namespace SupportChat.Backend.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync(long companyId);
}

public class DashboardStats
{
    public int TotalChats { get; set; }
    public int ActiveChats { get; set; }
    public int WaitingChats { get; set; }
    public int ClosedChats { get; set; }
    public int OnlineAgents { get; set; }
    public int TotalAgents { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public int TotalCustomers { get; set; }
}
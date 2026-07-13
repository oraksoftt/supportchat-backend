using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Requests;

public class CreateChatRequest
{
    public long CompanyId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; } = string.Empty;
    public string? IpAddress { get; set; } = string.Empty;
    public string? Country { get; set; } = string.Empty;
    public string? City { get; set; } = string.Empty;
    public string? Device { get; set; } = string.Empty;
    public string? Browser { get; set; } = string.Empty;
    public string? OperatingSystem { get; set; } = string.Empty;
    public long? DepartmentId { get; set; } = 0;
    public string? Subject { get; set; } = string.Empty;
}

public class AssignChatRequest
{
    public long ChatId { get; set; }
    public long AgentId { get; set; }
}

public class CloseChatRequest
{
    public long ChatId { get; set; }
}

public class TransferChatRequest
{
    public long ChatId { get; set; }
    public long NewAgentId { get; set; }
}
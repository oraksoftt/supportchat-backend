using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Responses;

public class ChatResponse
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public long? AssignedAgentId { get; set; }
    public string? AgentName { get; set; }
    public ChatStatus Status { get; set; }
    public ChatPriority Priority { get; set; }
    public string? Subject { get; set; }
    public DateTime StartedOn { get; set; }
    public DateTime? ClosedOn { get; set; }
    public DateTime? LastMessageOn { get; set; }
    public int MessageCount { get; set; }
}
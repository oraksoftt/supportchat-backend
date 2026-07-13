using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Domain;

using System.Text.Json.Serialization;

public class Chat
{
    public long Id { get; set; }

    public long CompanyId { get; set; }

    public long CustomerId { get; set; }

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;

    public long? DepartmentId { get; set; }

    public long? AssignedAgentId { get; set; }

    [JsonPropertyName("agentName")]
    public string AgentName { get; set; } = string.Empty;

    // Enums serialize as integers by default. 
    // If you want them as strings (e.g., "Open" instead of 1), see the bonus note below.
    public ChatStatus Status { get; set; }
    public ChatPriority Priority { get; set; }

    public string? Subject { get; set; }

    public DateTime StartedOn { get; set; }
    public DateTime? ClosedOn { get; set; }
    public DateTime? LastMessageOn { get; set; }

    // Audit & Soft Delete properties
    public long? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }

    [JsonIgnore] // Exclude from JSON response entirely to keep API payloads secure/clean
    public long? DeletedBy { get; set; }

    [JsonIgnore]
    public DateTime? DeletedOn { get; set; }

    [JsonIgnore]
    public bool IsDeleted { get; set; }

    [JsonPropertyName("messageCount")]
    public int MessageCount { get; set; } = 0;
}
namespace SupportChat.Backend.Models.Domain;

public class ChatAssignment
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public long CompanyId { get; set; }
    public long AgentId { get; set; }
    public DateTime AssignedOn { get; set; }
    public DateTime? UnassignedOn { get; set; }
    public bool IsActive { get; set; }
}
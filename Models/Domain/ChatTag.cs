namespace SupportChat.Backend.Models.Domain;

public class ChatTag
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}
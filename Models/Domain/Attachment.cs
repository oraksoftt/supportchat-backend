namespace SupportChat.Backend.Models.Domain;

public class Attachment
{
    public long Id { get; set; }
    public long MessageId { get; set; }
    public long CompanyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public bool IsDeleted { get; set; }
}
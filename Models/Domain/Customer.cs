namespace SupportChat.Backend.Models.Domain;

public class Customer
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Device { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedOn { get; set; }
}
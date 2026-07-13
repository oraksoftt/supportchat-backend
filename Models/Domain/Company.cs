namespace SupportChat.Backend.Models.Domain;

public class Company
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Timezone { get; set; }
    public string? DefaultLanguage { get; set; }
    public long? SubscriptionPlanId { get; set; }
    public DateTime? SubscriptionExpiry { get; set; }
    public bool IsActive { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public long? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public bool IsDeleted { get; set; }
}
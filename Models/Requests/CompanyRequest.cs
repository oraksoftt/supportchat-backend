namespace SupportChat.Backend.Models.Requests;

public class CreateCompanyRequest
{
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
    public long? CreatedBy { get; set; }
}

public class UpdateCompanyRequest
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Timezone { get; set; }
    public string? DefaultLanguage { get; set; }
    public long? ModifiedBy { get; set; }
}
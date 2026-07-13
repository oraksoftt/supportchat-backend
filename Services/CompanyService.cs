using SupportChat.Backend.Models.Domain;
using System.Linq;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ICompanyRepository companyRepo,
        IAuditRepository auditRepo,
        ILogger<CompanyService> logger)
    {
        _companyRepo = companyRepo;
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task<Company> GetByIdAsync(long companyId)
    {
        return await _companyRepo.GetCompanyByIdAsync(companyId);
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _companyRepo.GetAllCompaniesAsync();
    }

    public async Task<long> CreateAsync(CreateCompanyRequest request)
    {
        // Validate email uniqueness (simple implementation using existing repo methods)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var companyExist = await _companyRepo.GetCompanyByEmailAsync(request.Email);

            if (companyExist != null)
            {
                throw new BadHttpRequestException("Email is already registered for another company.");
            }
        }

        // Basic subscription plan validation: ensure id is positive when provided
        if (request.SubscriptionPlanId.HasValue && request.SubscriptionPlanId <= 0)
        {
            throw new InvalidOperationException("Invalid subscription plan id.");
        }

        // If a subscription plan is provided but no expiry, set a default expiry (30 days)
        if (request.SubscriptionPlanId.HasValue && !request.SubscriptionExpiry.HasValue)
        {
            request.SubscriptionExpiry = DateTime.UtcNow.AddDays(30);
        }
        var company = new Company
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Phone = request.Phone,
            Website = request.Website,
            LogoUrl = request.LogoUrl,
            Timezone = request.Timezone,
            DefaultLanguage = request.DefaultLanguage,
            SubscriptionPlanId = request.SubscriptionPlanId,
            SubscriptionExpiry = request.SubscriptionExpiry,
            CreatedBy = request.CreatedBy
        };

        var companyId = await _companyRepo.CreateCompanyAsync(company);

        

        await _auditRepo.CreateAuditLogAsync(
            companyId,
            request.CreatedBy,
            "CompanyCreated",
            "Company",
            companyId,
            null,
            $"Name={request.Name}, Email={request.Email}",
            null,
            null
        );

        return companyId;
    }

    public async Task UpdateAsync(UpdateCompanyRequest request)
    {
        var existing = await _companyRepo.GetCompanyByIdAsync(request.CompanyId);
        if (existing == null)
        {
            throw new InvalidOperationException("Company not found.");
        }

        existing.Name = request.Name;
        existing.DisplayName = request.DisplayName;
        existing.Email = request.Email;
        existing.Phone = request.Phone;
        existing.Website = request.Website;
        existing.LogoUrl = request.LogoUrl;
        existing.Timezone = request.Timezone;
        existing.DefaultLanguage = request.DefaultLanguage;
        existing.ModifiedBy = request.ModifiedBy;
        existing.ModifiedOn = DateTime.UtcNow;

        await _companyRepo.UpdateCompanyAsync(existing);

        await _auditRepo.CreateAuditLogAsync(
            request.CompanyId,
            request.ModifiedBy,
            "CompanyUpdated",
            "Company",
            request.CompanyId,
            System.Text.Json.JsonSerializer.Serialize(existing),
            System.Text.Json.JsonSerializer.Serialize(request),
            null,
            null
        );
    }

    public async Task DeleteAsync(long companyId, long deletedBy)
    {
        await _companyRepo.DeleteCompanyAsync(companyId, deletedBy);

        await _auditRepo.CreateAuditLogAsync(
            companyId,
            deletedBy,
            "CompanyDeleted",
            "Company",
            companyId,
            null,
            $"Deleted by UserId={deletedBy}",
            null,
            null
        );
    }
}
namespace SupportChat.Backend.Models.Domain;

public class User
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long? DepartmentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public long? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public bool IsDeleted { get; set; }
}

// Helper DTOs
public record GetUsersRequest(long? CompanyId);
public record CreateUserRequest(
    long CompanyId,
    long? DepartmentId,
    string FirstName,
    string LastName,
    string Email,
    string? Mobile,
    string Password,
    string[] Roles
);
public record UpdateUserRequest(
    long UserId,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Mobile,
    bool IsActive,
    long? DepartmentId
);
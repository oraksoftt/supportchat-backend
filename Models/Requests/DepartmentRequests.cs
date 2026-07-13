namespace SupportChat.Backend.Models.Requests;

public class CreateDepartmentRequest
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? CreatedBy { get; set; }
}

public class UpdateDepartmentRequest
{
    public long DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? ModifiedBy { get; set; }
}
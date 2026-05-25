namespace PARKit.Backend.DTOs
{
    public class CompanyDto {
    public int Id { get; set; }
    public string NameCompany { get; set; } = string.Empty;
    public string CIF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; } 
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}
}
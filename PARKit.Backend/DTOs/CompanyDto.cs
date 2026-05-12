namespace PARKit.Backend.DTOs
{
    public class CompanyDto {
    public int Id { get; set; }
    public string NameCompany { get; set; } = string.Empty;
    public string CIF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
}
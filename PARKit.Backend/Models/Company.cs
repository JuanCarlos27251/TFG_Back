using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.Models
{
    public class Company
    {
        [Key]
        public int Id {get;set;}
        public string NameCompany {get;set;} = string.Empty;
        [Required]
        public string CIF {get;set;} = string.Empty;
        public string? Phone {get;set;}
        public bool IsActive{get;set;}
        [Required]
        public string Email {get;set;}  = string.Empty;
        [Required]
        public string PasswordHash{get;set;} = string.Empty;

    }
}
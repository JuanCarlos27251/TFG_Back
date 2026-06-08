using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.DTOs.UserDtin
{
    public class UserDtin
    {
        [Required]
        public string Name {get;set;} = string.Empty;
        [Required,EmailAddress]
        public string Email {get;set;} = string.Empty;
        
        public string? Password{get;set;} = string.Empty;
        public string? Phone {get;set;}
    }
}

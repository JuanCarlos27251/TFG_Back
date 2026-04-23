using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.DTOs.UserDto
{
    public class UserDto
    {
        [Key]
        public int Id {get;set;}
        [Required]
        public string Name {get;set;} = string.Empty;
        [Required]
        public string Email {get;set;} = string.Empty;
        public DateTime CreatedAT {get;set;} = DateTime.UtcNow;
        public bool IsActive{get;set;} = true;
        public string? Phone {get;set;}
        [Required]
        public string Role {get;set;} = string.Empty;
    }
}
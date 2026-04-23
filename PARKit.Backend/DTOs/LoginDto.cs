using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.DTOs.LoginDto
{
    public class LoginDtin
    {
        [Required]
        public string Name {get;set;} = string.Empty;
        [Required]
        public string Email {get;set;} = string.Empty;
        [Required]
        public string PasswordHash{get;set;} = string.Empty;
    }
}
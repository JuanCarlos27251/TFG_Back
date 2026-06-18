using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.DTOs
{
    public class LoginDtin
    {

        [Required,EmailAddress]
        public string Email {get;set;} = string.Empty;
        [Required]
        public string Password{get;set;} = string.Empty;
    }
}
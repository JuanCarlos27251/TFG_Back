using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PARKit.Backend.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id {get;set;}

        [ForeignKey("User")]
        public int UserId {get;set;}
        [Required]
        public string CadType {get;set;} =string.Empty;

        [Required]
        [MaxLength(4)]
        public string LastFourDigits { get; set; } = "0000";

        [Required]
        [MaxLength(100)]
        public string HolderName { get; set; } = string.Empty;

        [Required]
        public string ExpiryDate { get; set; } = string.Empty; 


    }
}
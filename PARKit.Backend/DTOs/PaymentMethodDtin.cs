using System.ComponentModel.DataAnnotations;

namespace PARKit.Backend.DTOs.PaymentMethodDtin
{
    public class PaymentMethodDtin
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string CadType { get; set; } = string.Empty; // Ej: "Visa", "Mastercard"

        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string LastFourDigits { get; set; } = "0000";

        [Required]
        public string HolderName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Formato MM/YY")]
        public string ExpiryDate { get; set; } = string.Empty;
    }
}
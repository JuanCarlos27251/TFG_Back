namespace PARKit.Backend.DTOs
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CadType { get; set; } = string.Empty;
        public string LastFourDigits { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
    }
}
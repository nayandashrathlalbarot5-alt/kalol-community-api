using System.ComponentModel.DataAnnotations;

namespace KalolCommunity.Contracts.DTO
{
    public class VerifyOtpRequestDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(255, ErrorMessage = "Email must be less than or equal to 255 characters.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "OTP is required.")]
        [RegularExpression("^\\d{6}$", ErrorMessage = "OTP must be a 6-digit number.")]
        public string Otp { get; set; } = null!;
    }
}

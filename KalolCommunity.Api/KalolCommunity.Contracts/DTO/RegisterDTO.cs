using System.ComponentModel.DataAnnotations;

namespace KalolCommunity.Contracts.DTO
{
    public class RegisterDTO
    {
        [MaxLength(50, ErrorMessage = "First name must be less than or equal to 50 characters.")]
        public string? FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "Last name must be less than or equal to 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(100, ErrorMessage = "Email must be less than or equal to 100 characters.")]
        public string Email { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "Password must be less than or equal to 50 characters.")]
        public string? Password { get; set; }

        [RegularExpression("^\\d{6}$", ErrorMessage = "OTP must be a 6-digit number.")]
        public string? Otp { get; set; }

        [RegularExpression("^[RrLl]$", ErrorMessage = "Flag must be 'R' or 'L'.")]
        public string? Flag { get; set; }
    }
}

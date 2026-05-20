using System.ComponentModel.DataAnnotations;

namespace KalolCommunity.Contracts.DTO
{
    public class SendOtpRequestDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(255, ErrorMessage = "Email must be less than or equal to 255 characters.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Flag is required.")]
        [RegularExpression("^[RrLl]$", ErrorMessage = "Flag must be 'R' or 'L'.")]
        public string Flag { get; set; } = null!;
    }
}

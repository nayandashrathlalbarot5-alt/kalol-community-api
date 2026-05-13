using System.ComponentModel.DataAnnotations;

namespace KalolCommunity.Contracts.DTO
{
    public class ContactUsRequestDTO
    {
        [Required(ErrorMessage = "First Name is required.")]
        [MaxLength(100, ErrorMessage = "First name must be less than or equal to 100 characters.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required.")]
        [MaxLength(100, ErrorMessage = "Last name must be less than or equal to 100 characters.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [MaxLength(20, ErrorMessage = "Phone number must be less than or equal to 20 characters.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(255, ErrorMessage = "Email must be less than or equal to 255 characters.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Message is required.")]
        [MaxLength(5000, ErrorMessage = "Message must be less than or equal to 5000 characters.")]
        public string Message { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KalolCommunity.Contracts.DTO
{
    public class LoginDTO
    {
        // Email input from the user during login.
        [EmailAddress]
        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(100, ErrorMessage = "Email must be less than or equal to 100 characters.")]
        public string Email { get; set; } = null!;
        
        // Password input from the user during login.
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [MaxLength(50, ErrorMessage = "Password must be less than or equal to 100 characters.")]
        public string Password { get; set; } = null!;
    }
}

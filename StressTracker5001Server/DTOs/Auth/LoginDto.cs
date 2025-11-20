using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Auth
{
    public class LoginDto
    {

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 8,
            ErrorMessage = "Password must be between 8 and 128 characters")]
        public required string Password { get; set; }
    }
}

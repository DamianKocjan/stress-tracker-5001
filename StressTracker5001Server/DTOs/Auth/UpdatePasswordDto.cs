using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Auth
{
    public class UpdatePasswordDto
    {

        [Required(ErrorMessage = "Current password is required")]
        [StringLength(128, MinimumLength = 8,
            ErrorMessage = "Password must be between 8 and 128 characters")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(128, MinimumLength = 8,
            ErrorMessage = "New password must be between 8 and 128 characters")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password is required")]
        [StringLength(128, MinimumLength = 8,
            ErrorMessage = "Confirm new password must be between 8 and 128 characters")]
        public required string ConfirmNewPassword { get; set; }
    }
}

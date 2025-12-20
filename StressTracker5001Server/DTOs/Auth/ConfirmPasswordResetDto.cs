using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Auth
{
  public class ConfirmPasswordResetDto
  {
    [Required(ErrorMessage = "Reset token is required")]
    public required string Token { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [StringLength(128, MinimumLength = 8,
        ErrorMessage = "Password must be between 8 and 128 characters")]
    public required string NewPassword { get; set; }

    [Required(ErrorMessage = "Password confirmation is required")]
    public required string ConfirmPassword { get; set; }
  }
}

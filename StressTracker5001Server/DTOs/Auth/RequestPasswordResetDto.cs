using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Auth
{
  public class RequestPasswordResetDto
  {
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; set; }
  }
}

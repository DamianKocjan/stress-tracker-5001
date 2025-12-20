using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Auth
{
  public class RequestEmailChangeDto
  {
    [Required(ErrorMessage = "New email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string NewEmail { get; set; }

    [Required(ErrorMessage = "Password is required for security verification")]
    public required string Password { get; set; }
  }
}

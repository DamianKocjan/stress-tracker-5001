using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Auth
{
  public class ConfirmEmailChangeDto
  {
    [Required(ErrorMessage = "Email verification token is required")]
    public required string Token { get; set; }
  }
}

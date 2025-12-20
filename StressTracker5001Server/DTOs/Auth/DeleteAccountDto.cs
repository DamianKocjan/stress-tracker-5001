using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.Auth
{
  public class DeleteAccountDto
  {
    [Required(ErrorMessage = "Password is required for account deletion")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Confirmation is required")]
    public bool ConfirmDeletion { get; set; }
  }
}

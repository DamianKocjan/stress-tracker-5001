using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
  public class EmailVerificationToken
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Token is required")]
    [StringLength(128, ErrorMessage = "Token cannot exceed 128 characters")]
    public required string TokenHash { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Required(ErrorMessage = "Email to verify is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public required string EmailToVerify { get; set; }

    [Required(ErrorMessage = "Expiration time is required")]
    public DateTime ExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    [Required(ErrorMessage = "Created at is required")]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Updated at is required")]
    public DateTime UpdatedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsConfirmed => ConfirmedAt.HasValue;
    public bool IsValid => !IsExpired && !IsConfirmed;
  }
}

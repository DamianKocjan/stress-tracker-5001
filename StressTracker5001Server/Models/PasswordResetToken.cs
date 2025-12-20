using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
  public class PasswordResetToken
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Token is required")]
    [StringLength(128, ErrorMessage = "Token cannot exceed 128 characters")]
    public required string TokenHash { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Required(ErrorMessage = "Expiration time is required")]
    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    [Required(ErrorMessage = "Created at is required")]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Updated at is required")]
    public DateTime UpdatedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsValid => !IsExpired && !IsUsed;
  }
}

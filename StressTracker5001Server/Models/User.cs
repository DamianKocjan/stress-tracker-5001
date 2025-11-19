using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StressTracker5001Server.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 8,
            ErrorMessage = "Password must be between 8 and 128 characters")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-\s]+$",
            ErrorMessage = "Username can only contain letters, numbers, spaces, underscores, and hyphens")]
        public required string Username { get; set; }

        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public required string Token { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsActive => !RevokedAt.HasValue && !IsExpired;

        // Foreign key to User
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

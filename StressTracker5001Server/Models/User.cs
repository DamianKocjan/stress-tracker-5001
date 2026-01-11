using System.ComponentModel.DataAnnotations;

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
        public required string Username { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new();
        public List<Card> CreatedCards { get; set; } = new();
        public List<CardAssignment> CardAssignments { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public List<BoardMember> BoardMemberships { get; set; } = new();
        public List<BoardInvite> BoardInvites { get; set; } = new();
        public List<ActivityLog> ActivityLogs { get; set; } = new();
        public List<PasswordResetToken> PasswordResetTokens { get; set; } = new();
        public List<EmailVerificationToken> EmailVerificationTokens { get; set; } = new();
        public List<Attachment> UserAttachments { get; set; } = new();

        public bool EmailVerified { get; set; } = false;
        public string? PendingEmail { get; set; }
        public bool IsAccountActive { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

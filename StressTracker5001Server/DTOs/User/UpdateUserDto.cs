using System.ComponentModel.DataAnnotations;

namespace StressTracker5001Server.DTOs.User
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Username { get; set; }
    }
}

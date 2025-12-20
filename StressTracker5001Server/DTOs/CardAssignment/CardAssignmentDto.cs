using StressTracker5001Server.DTOs.User;

namespace StressTracker5001Server.DTOs.CardAssignment
{
    public class CardAssignmentDto
    {
        public required int Id { get; set; }
        public required int UserId { get; set; }
        public required UserDto User { get; set; }
        public required DateTime AssignedAt { get; set; }
    }
}

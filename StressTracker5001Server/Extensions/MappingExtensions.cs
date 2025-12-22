using StressTracker5001Server.DTOs.ActivityLog;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.DTOs.BoardInvite;
using StressTracker5001Server.DTOs.BoardMember;
using StressTracker5001Server.DTOs.Card;
using StressTracker5001Server.DTOs.CardAssignment;
using StressTracker5001Server.DTOs.Column;
using StressTracker5001Server.DTOs.Comment;
using StressTracker5001Server.DTOs.Tag;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Extensions
{
    public static class MappingExtensions
    {
        // ActivityLog mappings
        public static ActivityLogDto ToDto(this ActivityLog log)
        {
            return new ActivityLogDto
            {
                Id = log.Id,
                BoardId = log.BoardId,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                ActionType = log.Action,
                Description = log.Details,
                CreatedBy = log.User?.ToDto() ?? new UserDto
                {
                    Id = log.UserId,
                    Username = string.Empty,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue
                },
                CreatedAt = log.CreatedAt
            };
        }

        // User mappings
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public static UserDetailsDto ToDetailsDto(this User user)
        {
            return new UserDetailsDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        // Board mappings
        public static BoardDto ToDto(this Board board)
        {
            var owner = board.Members?.FirstOrDefault(m => m.Role == BoardMemberRole.Owner)?.User;
            return new BoardDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                Owner = owner?.ToDto() ?? new UserDto
                {
                    Id = 0,
                    Username = string.Empty,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue
                },
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt
            };
        }

        public static BoardDetailsDto ToDetailsDto(this Board board)
        {
            var owner = board.Members?.FirstOrDefault(m => m.Role == BoardMemberRole.Owner)?.User;
            return new BoardDetailsDto
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                Owner = owner?.ToDto() ?? new UserDto
                {
                    Id = 0,
                    Username = string.Empty,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue
                },
                Columns = board.Columns.Select(c => c.ToDto()).ToList(),
                Cards = board.Columns.SelectMany(c => c.Cards).Select(c => c.ToDto()).ToList(),
                Tags = board.Tags.Select(t => t.ToDto()).ToList(),
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt
            };
        }

        // Column mappings
        public static ColumnDto ToDto(this Column column)
        {
            return new ColumnDto
            {
                Id = column.Id,
                BoardId = column.BoardId,
                Name = column.Name,
                Position = column.Position,
                WipLimit = column.WipLimit,
                CreatedAt = column.CreatedAt,
                UpdatedAt = column.UpdatedAt
            };
        }

        // Card mappings
        public static CardDto ToDto(this Card card)
        {
            return new CardDto
            {
                Id = card.Id,
                ColumnId = card.ColumnId,
                Title = card.Title,
                Description = card.Description,
                Position = card.Position,
                DueDate = card.DueDate,
                CreatedById = card.CreatedById,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt,
                Tags = card.CardTags.Select(ct => ct.TagId).ToList(),
                Assignments = card.CardAssignments.Select(ca => ca.ToDto()).ToList(),
                AttachmentCount = card.Attachments.Count()
            };
        }

        public static CardDetailsDto ToDetailsDto(this Card card)
        {
            return new CardDetailsDto
            {
                Id = card.Id,
                ColumnId = card.ColumnId,
                Title = card.Title,
                Description = card.Description,
                Position = card.Position,
                DueDate = card.DueDate,
                CreatedById = card.CreatedById,
                CreatedBy = card.CreatedBy?.ToDto() ?? new UserDto
                {
                    Id = card.CreatedById,
                    Username = string.Empty,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue
                },
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt,
                Tags = card.CardTags.Select(ct => ct.TagId).ToList(),
                Assignments = card.CardAssignments.Select(ca => ca.ToDto()).ToList()
            };
        }

        // Tag mappings
        public static TagDto ToDto(this Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color,
                BoardId = tag.BoardId,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt
            };
        }

        // CardAssignment mappings
        public static CardAssignmentDto ToDto(this CardAssignment assignment)
        {
            return new CardAssignmentDto
            {
                Id = assignment.Id,
                UserId = assignment.UserId,
                User = assignment.User?.ToDto() ?? new UserDto
                {
                    Id = assignment.UserId,
                    Username = string.Empty,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue
                },
                AssignedAt = assignment.AssignedAt
            };
        }

        // Comment mappings
        public static CommentDto ToDto(this Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.UserId,
                User = comment.User?.ToDto() ?? new UserDto
                {
                    Id = comment.UserId,
                    Username = string.Empty,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue
                },
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        // BoardMember mappings
        public static BoardMemberDto ToDto(this BoardMember boardMember)
        {
            return new BoardMemberDto
            {
                Id = boardMember.Id,
                BoardId = boardMember.BoardId,
                UserId = boardMember.UserId,
                User = boardMember.User?.ToDto() ?? new UserDto
                {
                    Id = boardMember.UserId,
                    Username = string.Empty,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue
                },
                Role = boardMember.Role,
                CreatedAt = boardMember.CreatedAt,
                UpdatedAt = boardMember.UpdatedAt
            };
        }

        // BoardInvite mappings
        public static BoardInviteDto ToDto(this BoardInvite boardInvite)
        {
            return new BoardInviteDto
            {
                Id = boardInvite.Id,
                Token = boardInvite.Token,
                Role = (int)boardInvite.Role,
                CreatedAt = boardInvite.CreatedAt,
                GeneratedByUser = boardInvite.GeneratedByUser != null
                    ? boardInvite.GeneratedByUser.ToDto()
                    : new UserDto
                    {
                        Id = boardInvite.GeneratedByUserId,
                        Username = "Unknown",
                        CreatedAt = boardInvite.CreatedAt,
                        UpdatedAt = boardInvite.UpdatedAt
                    }
            };
        }

        // List mappings
        public static List<ActivityLogDto> ToDto(this IEnumerable<ActivityLog> activityLogs)
        {
            return activityLogs.Select(al => al.ToDto()).ToList();
        }

        public static List<BoardDto> ToDto(this IEnumerable<Board> boards)
        {
            return boards.Select(b => b.ToDto()).ToList();
        }

        public static List<ColumnDto> ToDto(this IEnumerable<Column> columns)
        {
            return columns.Select(c => c.ToDto()).ToList();
        }

        public static List<CardDto> ToDto(this IEnumerable<Card> cards)
        {
            return cards.Select(c => c.ToDto()).ToList();
        }

        public static List<TagDto> ToDto(this IEnumerable<Tag> tags)
        {
            return tags.Select(t => t.ToDto()).ToList();
        }

        public static List<CommentDto> ToDto(this IEnumerable<Comment> comments)
        {
            return comments.Select(c => c.ToDto()).ToList();
        }

        public static List<BoardMemberDto> ToDto(this IEnumerable<BoardMember> boardMembers)
        {
            return boardMembers.Select(bm => bm.ToDto()).ToList();
        }

        public static List<BoardInviteDto> ToDto(this IEnumerable<BoardInvite> boardInvites)
        {
            return boardInvites.Select(bi => bi.ToDto()).ToList();
        }
    }
}

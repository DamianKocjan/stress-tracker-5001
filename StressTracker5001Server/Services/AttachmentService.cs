using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Common;
using StressTracker5001Server.Data;
using StressTracker5001Server.DTOs.Attachment;
using StressTracker5001Server.Extensions;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface IAttachmentService
    {
        Task<Result<AttachmentDto>> UploadAttachmentAsync(int cardId, IFormFile file, int userId);
        Task<Result<AttachmentDto>> GetAttachmentAsync(Guid id, int userId);
        Task<Result<bool>> DeleteAttachmentAsync(Guid id, int userId);
        Task<Result<List<AttachmentDto>>> GetCardAttachmentsAsync(int cardId, int userId);
    }

    public class AttachmentService : IAttachmentService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IBoardAuthorizationService _boardAuthorizationService;
        private readonly IConfiguration _configuration;

        private readonly int _maxFileSizeMB;
        private readonly int _maxFileSizeBytes;
        private readonly string[] _allowedExtensions;

        public AttachmentService(
            AppDbContext context,
            IFileStorageService fileStorageService,
            IBoardAuthorizationService boardAuthorizationService,
            IConfiguration configuration)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _boardAuthorizationService = boardAuthorizationService;
            _configuration = configuration;


            _maxFileSizeMB = _configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 10);
            _maxFileSizeBytes = _maxFileSizeMB * 1024 * 1024;
            _allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
                ?? [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".txt", ".zip", ".xlsx", ".pptx"];
        }

        public async Task<Result<AttachmentDto>> UploadAttachmentAsync(int cardId, IFormFile file, int userId)
        {
            // Validate file is provided
            if (file == null || file.Length == 0)
            {
                return Result<AttachmentDto>.Failure("No file provided");
            }

            // Verify card exists
            var card = await _context.Cards
                .Include(c => c.Column)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
            {
                return Result<AttachmentDto>.NotFound("Card not found");
            }

            if (card.Column == null)
            {
                return Result<AttachmentDto>.Failure("Card column not found");
            }

            // Verify user has access to the board
            var hasAccess = await _boardAuthorizationService.UserCanAccessBoardAsync(card.Column.BoardId, userId);
            if (!hasAccess)
            {
                return Result<AttachmentDto>.Forbidden("Access denied to board");
            }

            // Validate file size
            if (file.Length > _maxFileSizeBytes)
            {
                return Result<AttachmentDto>.Failure($"File size exceeds {_maxFileSizeMB} MB limit");
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                return Result<AttachmentDto>.Failure($"File type '{fileExtension}' is not allowed");
            }

            try
            {
                // Sanitize filename to keep only the name with extension
                var sanitizedFileName = Path.GetFileName(file.FileName);

                // Create attachment record first
                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    CardId = cardId,
                    FileName = sanitizedFileName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    FileSize = file.Length,
                    UploadedById = userId,
                    UploadedAt = DateTime.UtcNow
                };

                using var stream = file.OpenReadStream();
                var uploadSuccess = await _fileStorageService.UploadFileAsync(
                    attachment.Id,
                    stream,
                    sanitizedFileName,
                    file.ContentType ?? "application/octet-stream");

                if (!uploadSuccess)
                {
                    return Result<AttachmentDto>.Failure("Failed to upload file to storage");
                }

                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();

                var fileUrl = _fileStorageService.GetFileUrl(attachment.Id);
                var uploadedBy = await _context.Users.FindAsync(attachment.UploadedById);

                var responseDto = new AttachmentDto
                {
                    Id = attachment.Id,
                    CardId = attachment.CardId,
                    FileName = attachment.FileName,
                    ContentType = attachment.ContentType,
                    FileSize = attachment.FileSize,
                    UploadedById = attachment.UploadedById,
                    UploadedBy = uploadedBy!.ToDto(),
                    UploadedAt = attachment.UploadedAt,
                    FileUrl = fileUrl
                };

                return Result<AttachmentDto>.Success(responseDto);
            }
            catch
            {
                return Result<AttachmentDto>.Failure("Failed to upload attachment");
            }
        }

        public async Task<Result<AttachmentDto>> GetAttachmentAsync(Guid id, int userId)
        {
            var attachment = await _context.Attachments
                .Include(a => a.Card)
                .ThenInclude(c => c!.Column)
                .Include(a => a.UploadedBy)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null)
            {
                return Result<AttachmentDto>.NotFound("Attachment not found");
            }

            if (attachment.Card?.Column == null)
            {
                return Result<AttachmentDto>.Failure("Card or column not found");
            }

            // Verify user has access to the board
            var hasAccess = await _boardAuthorizationService.UserCanAccessBoardAsync(
                attachment.Card.Column.BoardId,
                userId);

            if (!hasAccess)
            {
                return Result<AttachmentDto>.Forbidden("Access denied to board");
            }

            var fileUrl = _fileStorageService.GetFileUrl(attachment.Id);

            var responseDto = new AttachmentDto
            {
                Id = attachment.Id,
                CardId = attachment.CardId,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedById = attachment.UploadedById,
                UploadedBy = attachment.UploadedBy!.ToDto(),
                UploadedAt = attachment.UploadedAt,
                FileUrl = fileUrl
            };

            return Result<AttachmentDto>.Success(responseDto);
        }

        public async Task<Result<bool>> DeleteAttachmentAsync(Guid id, int userId)
        {
            var attachment = await _context.Attachments
                .Include(a => a.Card)
                .ThenInclude(c => c!.Column)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null)
            {
                return Result<bool>.NotFound("Attachment not found");
            }

            if (attachment.Card?.Column == null)
            {
                return Result<bool>.Failure("Card or column not found");
            }

            // Verify user has access to the board
            var hasAccess = await _boardAuthorizationService.UserCanAccessBoardAsync(
                attachment.Card.Column.BoardId,
                userId);

            if (!hasAccess)
            {
                return Result<bool>.Forbidden("Access denied to board");
            }

            // Check if user is the uploader or has admin access
            if (attachment.UploadedById != userId)
            {
                var roleResult = await _boardAuthorizationService.GetBoardUserRoleByIdAsync(
                    attachment.Card.Column.BoardId,
                    userId);

                if (!roleResult.IsSuccess || (roleResult.Value != BoardMemberRole.Admin && roleResult.Value != BoardMemberRole.Owner))
                {
                    return Result<bool>.Forbidden("Only the uploader or board admin can delete attachments");
                }
            }

            try
            {
                var deleteSuccess = await _fileStorageService.DeleteFileAsync(attachment.Id);
                if (!deleteSuccess)
                {
                    return Result<bool>.Failure("Failed to delete file from storage");
                }

                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch
            {
                return Result<bool>.Failure("Failed to delete attachment");
            }
        }

        public async Task<Result<List<AttachmentDto>>> GetCardAttachmentsAsync(int cardId, int userId)
        {
            var card = await _context.Cards
                .Include(c => c.Column)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
            {
                return Result<List<AttachmentDto>>.NotFound("Card not found");
            }

            if (card.Column == null)
            {
                return Result<List<AttachmentDto>>.Failure("Card column not found");
            }

            // Verify user has access to the board
            var hasAccess = await _boardAuthorizationService.UserCanAccessBoardAsync(card.Column.BoardId, userId);
            if (!hasAccess)
            {
                return Result<List<AttachmentDto>>.Forbidden("Access denied to board");
            }

            var attachments = await _context.Attachments
                .Include(a => a.UploadedBy)
                .Where(a => a.CardId == cardId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();

            var responseDtos = attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                CardId = a.CardId,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedById = a.UploadedById,
                UploadedBy = a.UploadedBy!.ToDto(),
                UploadedAt = a.UploadedAt,
                FileUrl = _fileStorageService.GetFileUrl(a.Id)
            }).ToList();

            return Result<List<AttachmentDto>>.Success(responseDtos);
        }
    }
}

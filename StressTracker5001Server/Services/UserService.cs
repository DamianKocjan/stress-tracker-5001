using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Data;
using StressTracker5001Server.Common;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Claims;

namespace StressTracker5001Server.Services
{
    public interface IUserService
    {
        Task<Result<User>> GetUserByIdAsync(int id);
        Task<Result<User>> GetUserByEmailAsync(string email);
        bool VerifyPassword(User user, string password);
        Task<Result<User>> UpdateUserPasswordAsync(int id, string newPassword);
        Task<Result<User>> CreateUserAsync(CreateUserDto dto);
        Task<Result<User>> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<Result<bool>> DeleteUserAsync(int id);
        Task<Result<(string Token, string ResetLink)>> RequestPasswordResetAsync(string email, string baseUrl);
        Task<Result<bool>> ConfirmPasswordResetAsync(string token, string newPassword);
        Task<Result<(string Token, string VerificationLink)>> RequestEmailChangeAsync(int userId, string newEmail, string baseUrl);
        Task<Result<bool>> ConfirmEmailChangeAsync(string token);
        Task<Result<bool>> SoftDeleteAccountAsync(int userId);
        Task<Result<(string Token, string VerificationLink)>> ResendEmailVerificationAsync(int userId, string baseUrl);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Result<User>> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result<User>.NotFound($"User not found");
            }
            return Result<User>.Success(user);
        }

        public async Task<Result<User>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(U => U.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                return Result<User>.NotFound($"User not found");
            }
            return Result<User>.Success(user);
        }

        public bool VerifyPassword(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<Result<User>> UpdateUserPasswordAsync(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result<User>.NotFound($"User not found");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<User>.Success(user);
        }

        public async Task<Result<User>> CreateUserAsync(CreateUserDto dto)
        {
            // Check if user with email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existingUser != null)
            {
                return Result<User>.Failure("User with this email already exists", 400);
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Username = dto.Username,
                CreatedAt = now,
                UpdatedAt = now,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Result<User>.Success(user);
        }

        public async Task<Result<User>> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result<User>.NotFound($"User not found");
            }

            user.Username = dto.Username;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<User>.Success(user);
        }

        public async Task<Result<bool>> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result<bool>.NotFound($"User not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<(string Token, string ResetLink)>> RequestPasswordResetAsync(string email, string baseUrl)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                // Return success even if user doesn't exist for security reasons (prevent email enumeration)
                return Result<(string, string)>.Success(("", ""));
            }

            var (token, tokenHash) = GenerateSecureToken();
            var expiryMinutes = _configuration.GetValue<int>("Auth:PasswordReset:TokenExpiryMinutes", 60);

            var resetToken = new PasswordResetToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            var resetLink = $"{baseUrl}/reset-password/{token}";
            return Result<(string, string)>.Success((token, resetLink));
        }

        public async Task<Result<bool>> ConfirmPasswordResetAsync(string token, string newPassword)
        {
            var tokenHash = HashToken(token);
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(prt => prt.TokenHash == tokenHash);

            if (resetToken == null || !resetToken.IsValid)
            {
                return Result<bool>.Failure("Invalid or expired password reset token", 400);
            }

            var user = await _context.Users.FindAsync(resetToken.UserId);
            if (user == null)
            {
                return Result<bool>.NotFound("User not found");
            }

            // Hash new password and update user
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Mark token as used
            resetToken.UsedAt = DateTime.UtcNow;
            resetToken.UpdatedAt = DateTime.UtcNow;

            _context.PasswordResetTokens.Update(resetToken);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<(string Token, string VerificationLink)>> RequestEmailChangeAsync(int userId, string newEmail, string baseUrl)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Result<(string, string)>.NotFound("User not found");
            }

            if (user.Email.ToLower() == newEmail.ToLower())
            {
                return Result<(string, string)>.Failure("New email cannot be the same as the current email", 400);
            }

            // Check if new email is already in use
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == newEmail.ToLower());
            if (existingUser != null)
            {
                return Result<(string, string)>.Failure("Email already in use", 400);
            }

            var (token, tokenHash) = GenerateSecureToken();
            var expiryMinutes = _configuration.GetValue<int>("Auth:EmailVerification:TokenExpiryMinutes", 1440);

            var verificationToken = new EmailVerificationToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                EmailToVerify = newEmail,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EmailVerificationTokens.Add(verificationToken);
            user.PendingEmail = newEmail;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var verificationLink = $"{baseUrl}/confirm-email/{token}";
            return Result<(string, string)>.Success((token, verificationLink));
        }

        public async Task<Result<bool>> ConfirmEmailChangeAsync(string token)
        {
            var tokenHash = HashToken(token);
            var verificationToken = await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(evt => evt.TokenHash == tokenHash);

            if (verificationToken == null || !verificationToken.IsValid)
            {
                return Result<bool>.Failure("Invalid or expired email verification token", 400);
            }

            var user = await _context.Users.FindAsync(verificationToken.UserId);
            if (user == null)
            {
                return Result<bool>.NotFound("User not found");
            }

            // Update user email and mark as verified
            user.Email = verificationToken.EmailToVerify;
            user.EmailVerified = true;
            user.PendingEmail = null;
            user.UpdatedAt = DateTime.UtcNow;

            // Mark token as confirmed
            verificationToken.ConfirmedAt = DateTime.UtcNow;
            verificationToken.UpdatedAt = DateTime.UtcNow;

            _context.EmailVerificationTokens.Update(verificationToken);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> SoftDeleteAccountAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Result<bool>.NotFound("User not found");
            }

            // Soft delete - set IsAccountActive to false
            user.IsAccountActive = false;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<(string Token, string VerificationLink)>> ResendEmailVerificationAsync(int userId, string baseUrl)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Result<(string, string)>.NotFound("User not found");
            }

            if (user.EmailVerified)
            {
                return Result<(string, string)>.Failure("Email is already verified", 400);
            }

            // Invalidate existing tokens
            var existingTokens = await _context.EmailVerificationTokens
                .Where(evt => evt.UserId == userId && !evt.IsConfirmed)
                .ToListAsync();

            foreach (var token in existingTokens)
            {
                token.ConfirmedAt = DateTime.UtcNow; // Mark as "used" by setting ConfirmedAt
                token.UpdatedAt = DateTime.UtcNow;
            }

            _context.EmailVerificationTokens.UpdateRange(existingTokens);

            // Generate new token
            var (newToken, tokenHash) = GenerateSecureToken();
            var expiryMinutes = _configuration.GetValue<int>("Auth:EmailVerification:TokenExpiryMinutes", 1440);

            var verificationToken = new EmailVerificationToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                EmailToVerify = user.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EmailVerificationTokens.Add(verificationToken);
            await _context.SaveChangesAsync();

            var verificationLink = $"{baseUrl}/verify-email/{newToken}";
            return Result<(string, string)>.Success((newToken, verificationLink));
        }

        private (string Token, string Hash) GenerateSecureToken()
        {
            var tokenChars = _configuration.GetValue<string>("Auth:TokenChars", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz");
            var tokenLength = _configuration.GetValue<int>("Auth:TokenLength", 32);

            var token = new char[tokenLength];
            var tokenBytes = new byte[tokenLength];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(tokenBytes);
            }

            for (int i = 0; i < token.Length; i++)
            {
                token[i] = tokenChars[tokenBytes[i] % tokenChars.Length];
            }

            var plainToken = new string(token);
            var hash = HashToken(plainToken);

            return (plainToken, hash);
        }

        private string HashToken(string token)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}

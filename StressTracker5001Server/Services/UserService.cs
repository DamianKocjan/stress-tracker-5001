using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Data;
using StressTracker5001Server.Common;
using Microsoft.EntityFrameworkCore;

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
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<User>> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result<User>.NotFound($"User with ID {id} not found");
            }
            return Result<User>.Success(user);
        }

        public async Task<Result<User>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(U => U.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                return Result<User>.NotFound($"User with email {email} not found");
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
                return Result<User>.NotFound($"User with ID {id} not found");
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
                return Result<User>.NotFound($"User with ID {id} not found");
            }

            // Check if email is being changed to an existing email
            if (!string.IsNullOrEmpty(dto.Email) && !dto.Email.Equals(user.Email, StringComparison.CurrentCultureIgnoreCase))
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
                if (existingUser != null)
                {
                    return Result<User>.Failure("User with this email already exists", 400);
                }
                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Username))
            {
                user.Username = dto.Username;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<User>.Success(user);
        }

        public async Task<Result<bool>> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result<bool>.NotFound($"User with ID {id} not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}

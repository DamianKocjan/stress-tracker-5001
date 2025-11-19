using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.User;
using StressTracker5001Server.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace StressTracker5001Server.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        bool VerifyPassword(User user, string password);
        Task<User?> UpdateUserPasswordAsync(int id, string newPassword);
        Task<User?> CreateUserAsync(CreateUserDto dto);
        Task<User?> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int id);
        Task SaveRefreshTokenAsync(int userId, RefreshToken refreshToken);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FindAsync(email.ToLower());
        }

        public bool VerifyPassword(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<User?> UpdateUserPasswordAsync(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> CreateUserAsync(CreateUserDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Username = dto.Username,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(dto.Username))
            {
                user.Username = dto.Username;
            }

            if (!string.IsNullOrEmpty(dto.Email))
            {
                user.Email = dto.Email;
            }

            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveRefreshTokenAsync(int userId, RefreshToken refreshToken)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            refreshToken.UserId = userId;
            refreshToken.CreatedAt = DateTime.UtcNow;
            refreshToken.UpdatedAt = DateTime.UtcNow;
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}

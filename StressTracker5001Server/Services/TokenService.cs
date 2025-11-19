using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Services
{
    public interface ITokenService
    {
        string GenerateToken(int userId, string email, string username);
        RefreshToken GenerateRefreshToken();
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private System.Security.Cryptography.RandomNumberGenerator _randomNumberGenerator;

        public TokenService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
            _randomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator.Create();
        }

        public string GenerateToken(int userId, string email, string username)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, username),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            _randomNumberGenerator.GetBytes(randomNumber);
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.Token == refreshToken && rt.ExpiresAt > DateTime.UtcNow && rt.RevokedAt == null)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Where(rt => rt.Token == refreshToken && rt.RevokedAt == null)
                .FirstOrDefaultAsync();

            if (token == null)
            {
                return false;
            }

            token.RevokedAt = DateTime.UtcNow;
            token.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

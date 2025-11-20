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
        string? GetTokenFromRequest(HttpRequest request);
        string? GetRefreshTokenFromRequest(HttpRequest request);
        void RemoveTokensFromResponse(HttpResponse response);
        bool ValidateToken(string token);
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task SaveRefreshTokenAsync(int userId, RefreshToken refreshToken);
        void ApplyTokensToResponse(HttpResponse response, string token, string refreshToken);
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

        public string? GetTokenFromRequest(HttpRequest request)
        {
            if (request.Cookies.TryGetValue(
                _configuration["Jwt:AuthTokenCookieName"] ?? "auth-token", out var token))
            {
                return token;
            }
            return null;
        }

        public string? GetRefreshTokenFromRequest(HttpRequest request)
        {
            if (request.Cookies.TryGetValue(
                _configuration["Jwt:RefreshTokenCookieName"] ?? "refresh-token", out var refreshToken))
            {
                return refreshToken;
            }
            return null;
        }

        public void RemoveTokensFromResponse(HttpResponse response)
        {
            var authTokenCookieName = _configuration["Jwt:AuthTokenCookieName"] ?? "auth-token";
            var refreshTokenCookieName = _configuration["Jwt:RefreshTokenCookieName"] ?? "refresh-token";

            response.Cookies.Delete(authTokenCookieName);
            response.Cookies.Delete(refreshTokenCookieName);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
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

        public async Task SaveRefreshTokenAsync(int userId, RefreshToken refreshToken)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            refreshToken.UserId = userId;

            var refreshTokenCookieExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenCookieExpiryDays);

            refreshToken.CreatedAt = DateTime.UtcNow;
            refreshToken.UpdatedAt = DateTime.UtcNow;

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public void ApplyTokensToResponse(HttpResponse response, string token, string refreshToken)
        {
            var authTokenCookieName = _configuration["Jwt:AuthTokenCookieName"] ?? "auth-token";
            var authTokenCookieExpiryHours = int.Parse(_configuration["Jwt:AuthTokenExpiryHours"] ?? "1");

            var refreshTokenCookieName = _configuration["Jwt:RefreshTokenCookieName"] ?? "refresh-token";
            var refreshTokenCookieExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");


            response.Cookies.Append(authTokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(authTokenCookieExpiryHours)
            });
            response.Cookies.Append(refreshTokenCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(refreshTokenCookieExpiryDays)
            });
        }
    }
}

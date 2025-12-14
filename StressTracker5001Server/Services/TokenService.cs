using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StressTracker5001Server.Common;
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
        Task<bool> ValidateTokenAsync(string token);
        Task<Result<RefreshToken>> GetRefreshTokenAsync(string refreshToken);
        Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken);
        Task<Result<bool>> SaveRefreshTokenAsync(int userId, RefreshToken refreshToken);
        void ApplyTokensToResponse(HttpResponse response, string token, string refreshToken);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly System.Security.Cryptography.RandomNumberGenerator _randomNumberGenerator;

        public TokenService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
            _randomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator.Create();
        }

        public string GenerateToken(int userId, string email, string username)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
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
            var now = DateTime.UtcNow;
            var randomNumber = new byte[32];
            _randomNumberGenerator.GetBytes(randomNumber);
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresAt = now.AddDays(7),
                CreatedAt = now,
                UpdatedAt = now,
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

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);

            try
            {
                await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Result<RefreshToken>> GetRefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Where(rt => rt.Token == refreshToken && rt.RevokedAt == null)
                .FirstOrDefaultAsync();

            if (token == null)
            {
                return Result<RefreshToken>.NotFound("Refresh token not found");
            }

            if (token.ExpiresAt <= DateTime.UtcNow)
            {
                return Result<RefreshToken>.Failure("Refresh token expired");
            }

            return Result<RefreshToken>.Success(token);
        }

        public async Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Where(rt => rt.Token == refreshToken && rt.RevokedAt == null)
                .FirstOrDefaultAsync();

            if (token == null)
            {
                return Result<bool>.NotFound("Refresh token not found");
            }

            token.RevokedAt = DateTime.UtcNow;
            token.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> SaveRefreshTokenAsync(int userId, RefreshToken refreshToken)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Result<bool>.NotFound("User not found");
            }

            refreshToken.UserId = userId;

            var now = DateTime.UtcNow;
            var refreshTokenCookieExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
            refreshToken.ExpiresAt = now.AddDays(refreshTokenCookieExpiryDays);

            refreshToken.CreatedAt = now;
            refreshToken.UpdatedAt = now;

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
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
                Expires = DateTime.UtcNow.AddHours(authTokenCookieExpiryHours)
            });
            response.Cookies.Append(refreshTokenCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(refreshTokenCookieExpiryDays)
            });
        }
    }
}

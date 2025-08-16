using Microsoft.EntityFrameworkCore;
using snic_api.Models;
using System.IdentityModel.Tokens.Jwt;

namespace snic_api.Services
{
    public class TokenBlacklistService
    {
        private readonly ApplicationDbContext _context;

        public TokenBlacklistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                    return false;

                return await _context.BlacklistedTokens
                    .AnyAsync(bt => bt.TokenId == jti && bt.ExpiresAt > DateTime.UtcNow);
            }
            catch
            {
                return false;
            }
        }

        public async Task BlacklistTokenAsync(string token, int userId, string reason = "Logout")
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                    return;

                // Check if token is already blacklisted
                var existingBlacklist = await _context.BlacklistedTokens
                    .FirstOrDefaultAsync(bt => bt.TokenId == jti);

                if (existingBlacklist != null)
                    return;

                var blacklistedToken = new BlacklistedToken
                {
                    TokenId = jti,
                    Token = token,
                    UserId = userId,
                    BlacklistedAt = DateTime.UtcNow,
                    ExpiresAt = jwtToken.ValidTo,
                    Reason = reason
                };

                _context.BlacklistedTokens.Add(blacklistedToken);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new InvalidOperationException("Failed to blacklist token", ex);
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.BlacklistedTokens
                    .Where(bt => bt.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _context.BlacklistedTokens.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new InvalidOperationException("Failed to cleanup expired tokens", ex);
            }
        }

        public async Task BlacklistAllUserTokensAsync(int userId, string reason = "Security")
        {
            try
            {
                // This would require storing active tokens or implementing a different strategy
                // For now, we'll just mark a flag or implement based on your needs
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to blacklist all user tokens", ex);
            }
        }
    }
} 
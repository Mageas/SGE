using Microsoft.EntityFrameworkCore;
using SGE.Application.Interfaces.Repositories;
using SGE.Core.Entities;
using SGE.Infrastructure.Data;

namespace SGE.Infrastructure.Repositories;

/// <summary>
///     Repository implementation for managing refresh tokens in the database.
///     Provides concrete implementations for refresh token operations including retrieval,
///     creation, update, revocation, and cleanup of expired tokens.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RefreshTokenRepository" /> class.
    /// </summary>
    /// <param name="context">The database context for accessing refresh token data.</param>
    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        return await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId &&
                         rt.RevokedAt == null &&
                         rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.Set<RefreshToken>().AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.Set<RefreshToken>().Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(string userId, string reason)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = reason;
        }

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoveExpiredTokensAsync()
    {
        var expiredTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        _context.Set<RefreshToken>().RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}

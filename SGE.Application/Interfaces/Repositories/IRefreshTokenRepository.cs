using SGE.Core.Entities;

namespace SGE.Application.Interfaces.Repositories;

/// <summary>
///     Repository interface for managing refresh tokens in the data store.
///     Provides methods for creating, retrieving, updating, and revoking refresh tokens.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    ///     Retrieves a refresh token by its token string value.
    /// </summary>
    /// <param name="token">The token string to search for.</param>
    /// <returns>The refresh token entity if found; otherwise, null.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    ///     Retrieves all active refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of active refresh tokens belonging to the user.</returns>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

    /// <summary>
    ///     Adds a new refresh token to the data store.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to add.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(RefreshToken refreshToken);

    /// <summary>
    ///     Updates an existing refresh token in the data store.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to update.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(RefreshToken refreshToken);

    /// <summary>
    ///     Revokes all active refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose tokens should be revoked.</param>
    /// <param name="reason">The reason for revoking the tokens.</param>
    /// <returns>A task representing the asynchronous revoke operation.</returns>
    Task RevokeAllUserTokensAsync(string userId, string reason);

    /// <summary>
    ///     Removes expired refresh tokens from the data store.
    /// </summary>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    Task RemoveExpiredTokensAsync();
}

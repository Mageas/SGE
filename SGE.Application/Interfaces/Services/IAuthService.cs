using SGE.Application.DTOs.Users;

namespace SGE.Application.Interfaces.Services;

/// <summary>
///     Interface defining authentication and authorization services for user management.
///     Provides methods for user registration, login, token refresh, and token revocation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    ///     Registers a new user in the system with the provided registration information.
    /// </summary>
    /// <param name="registerDto">The data transfer object containing user registration details.</param>
    /// <returns>An authentication response containing access token, refresh token, and user information.</returns>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    ///     Authenticates a user with their credentials and generates authentication tokens.
    /// </summary>
    /// <param name="loginDto">The data transfer object containing user login credentials.</param>
    /// <returns>An authentication response containing access token, refresh token, and user information.</returns>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

    /// <summary>
    ///     Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">The data transfer object containing the current access token and refresh token.</param>
    /// <returns>An authentication response containing new access token, refresh token, and user information.</returns>
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

    /// <summary>
    ///     Revokes a refresh token, preventing it from being used for token refresh operations.
    /// </summary>
    /// <param name="token">The refresh token to be revoked.</param>
    /// <returns>A task representing the asynchronous revoke operation.</returns>
    Task RevokeTokenAsync(string token);

    /// <summary>
    ///     Revokes all active refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous revoke operation.</returns>
    Task RevokeAllUserTokensAsync(string userId);

    /// <summary>
    ///     Retrieves user information for the specified user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A user data transfer object containing user details and roles.</returns>
    Task<UserDto> GetUserByIdAsync(string userId);
}

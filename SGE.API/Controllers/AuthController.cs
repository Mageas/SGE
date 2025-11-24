using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGE.Application.DTOs.Users;
using SGE.Application.Interfaces.Services;

namespace SGE.API.Controllers;

/// <summary>
///     Controller handling authentication and authorization operations.
///     Provides endpoints for user registration, login, token refresh, and token revocation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthController" /> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    ///     Registers a new user in the system.
    /// </summary>
    /// <param name="registerDto">The registration information.</param>
    /// <returns>An authentication response containing tokens and user information.</returns>
    /// <response code="200">User successfully registered.</response>
    /// <response code="400">Invalid registration data or user already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var response = await _authService.RegisterAsync(registerDto);
        return Ok(response);
    }

    /// <summary>
    ///     Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="loginDto">The login credentials.</param>
    /// <returns>An authentication response containing tokens and user information.</returns>
    /// <response code="200">User successfully authenticated.</response>
    /// <response code="401">Invalid credentials or inactive user.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var response = await _authService.LoginAsync(loginDto);
        return Ok(response);
    }

    /// <summary>
    ///     Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">The refresh token information.</param>
    /// <returns>An authentication response containing new tokens and user information.</returns>
    /// <response code="200">Token successfully refreshed.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var response = await _authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(response);
    }

    /// <summary>
    ///     Revokes a refresh token, preventing it from being used for future token refresh operations.
    /// </summary>
    /// <param name="revokeTokenDto">The token revocation information.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Token successfully revoked.</response>
    /// <response code="401">Invalid token or token already revoked.</response>
    [HttpPost("revoke-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto revokeTokenDto)
    {
        await _authService.RevokeTokenAsync(revokeTokenDto.Token);
        return NoContent();
    }

    /// <summary>
    ///     Retrieves the current authenticated user's information.
    /// </summary>
    /// <returns>The current user's details including roles.</returns>
    /// <response code="200">User information successfully retrieved.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    /// <summary>
    ///     Logs out the current user by revoking all their active refresh tokens.
    /// </summary>
    /// <returns>No content if successful.</returns>
    /// <response code="204">User successfully logged out.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Révoquer tous les tokens actifs de l'utilisateur
        await _authService.RevokeAllUserTokensAsync(userId);

        return NoContent();
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SGE.Application.DTOs.Users;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;
using SGE.Core.Exceptions;

namespace SGE.Application.Services;

/// <summary>
///     Service implementation for authentication and authorization operations.
///     Handles user registration, login, token generation, token refresh, and token revocation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthService" /> class.
    /// </summary>
    /// <param name="userManager">The user manager for handling user operations.</param>
    /// <param name="configuration">The configuration for accessing JWT settings.</param>
    /// <param name="refreshTokenRepository">The repository for managing refresh tokens.</param>
    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userManager = userManager;
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Vérifier si l'utilisateur existe déjà
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            throw new UserRegistrationException("Un utilisateur avec cet email existe déjà.");

        // Vérifier la correspondance des mots de passe
        if (registerDto.Password != registerDto.ConfirmPassword)
            throw new UserRegistrationException("Les mots de passe ne correspondent pas.");

        // Créer le nouvel utilisateur
        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            EmployeeId = registerDto.EmployeeId,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserRegistrationException($"Échec de l'inscription: {errors}");
        }

        // Assigner un rôle par défaut
        await _userManager.AddToRoleAsync(user, "User");

        // Générer les tokens
        return await GenerateAuthResponse(user);
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Trouver l'utilisateur par email
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !user.IsActive)
            throw new InvalidCredentialsException();

        // Vérifier le mot de passe
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        // Mettre à jour la dernière connexion
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Générer les tokens
        return await GenerateAuthResponse(user);
    }

    /// <inheritdoc />
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        // Valider l'access token (sans vérifier l'expiration)
        var principal = GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new InvalidRefreshTokenException("Token invalide.");

        // Récupérer l'utilisateur
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
            throw new UserNotFoundException(userId);

        // Valider le refresh token
        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);
        if (storedRefreshToken == null || !storedRefreshToken.IsActive || storedRefreshToken.UserId != userId)
            throw new InvalidRefreshTokenException();

        // Révoquer l'ancien refresh token
        storedRefreshToken.RevokedAt = DateTime.UtcNow;
        storedRefreshToken.ReasonRevoked = "Remplacé par un nouveau token";
        await _refreshTokenRepository.UpdateAsync(storedRefreshToken);

        // Générer de nouveaux tokens
        var response = await GenerateAuthResponse(user);
        storedRefreshToken.ReplacedByToken = response.RefreshToken;
        await _refreshTokenRepository.UpdateAsync(storedRefreshToken);

        return response;
    }

    /// <inheritdoc />
    public async Task RevokeTokenAsync(string token)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (refreshToken == null || !refreshToken.IsActive)
            throw new InvalidRefreshTokenException("Token invalide ou déjà révoqué.");

        // Révoquer le token
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReasonRevoked = "Révoqué par l'utilisateur";
        await _refreshTokenRepository.UpdateAsync(refreshToken);
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(string userId)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "Déconnexion de l'utilisateur");
    }

    /// <inheritdoc />
    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UserNotFoundException(userId);

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmployeeId = user.EmployeeId,
            Roles = roles
        };
    }

    /// <summary>
    ///     Generates an authentication response containing access token, refresh token, and user information.
    /// </summary>
    /// <param name="user">The user for whom to generate the authentication response.</param>
    /// <returns>An authentication response DTO containing tokens and user details.</returns>
    private async Task<AuthResponseDto> GenerateAuthResponse(ApplicationUser user)
    {
        var accessToken = await GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Sauvegarder le refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        // Récupérer les rôles de l'utilisateur
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                Roles = roles
            }
        };
    }

    /// <summary>
    ///     Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the access token.</param>
    /// <returns>A JWT access token string.</returns>
    private async Task<string> GenerateAccessToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Ajouter les rôles aux claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    ///     Generates a cryptographically secure random refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    ///     Extracts the claims principal from an expired JWT token without validating its expiration.
    /// </summary>
    /// <param name="token">The JWT token to parse.</param>
    /// <returns>The claims principal extracted from the token.</returns>
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = false, // Ne pas valider l'expiration
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new InvalidRefreshTokenException("Token invalide.");

        return principal;
    }

    /// <summary>
    ///     Gets the access token expiration time in minutes from configuration.
    /// </summary>
    /// <returns>The number of minutes until the access token expires.</returns>
    private int GetAccessTokenExpirationMinutes()
    {
        return _configuration.GetValue<int>("JwtSettings:AccessTokenExpiration", 60);
    }

    /// <summary>
    ///     Gets the refresh token expiration time in days from configuration.
    /// </summary>
    /// <returns>The number of days until the refresh token expires.</returns>
    private int GetRefreshTokenExpirationDays()
    {
        return _configuration.GetValue<int>("JwtSettings:RefreshTokenExpiration", 7);
    }
}

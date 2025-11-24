namespace SGE.Application.DTOs.Users;

/// <summary>
///     Data transfer object for token revocation requests.
/// </summary>
public class RevokeTokenDto
{
    /// <summary>
    ///     Gets or sets the refresh token to be revoked.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

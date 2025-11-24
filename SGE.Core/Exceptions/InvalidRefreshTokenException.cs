namespace SGE.Core.Exceptions;

/// <summary>
///     Exception thrown when a refresh token is invalid, expired, or revoked.
/// </summary>
public class InvalidRefreshTokenException : SgeException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidRefreshTokenException" /> class.
    /// </summary>
    public InvalidRefreshTokenException()
        : base("Refresh token invalide ou expiré.", "INVALID_REFRESH_TOKEN", 401)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidRefreshTokenException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidRefreshTokenException(string message)
        : base(message, "INVALID_REFRESH_TOKEN", 401)
    {
    }
}

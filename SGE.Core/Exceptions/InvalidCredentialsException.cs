namespace SGE.Core.Exceptions;

/// <summary>
///     Exception thrown when authentication credentials are invalid.
/// </summary>
public class InvalidCredentialsException : SgeException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCredentialsException" /> class.
    /// </summary>
    public InvalidCredentialsException()
        : base("Email ou mot de passe invalide.", "INVALID_CREDENTIALS", 401)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidCredentialsException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidCredentialsException(string message)
        : base(message, "INVALID_CREDENTIALS", 401)
    {
    }
}

namespace SGE.Core.Exceptions;

/// <summary>
///     Exception thrown when user registration fails.
/// </summary>
public class UserRegistrationException : SgeException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRegistrationException" /> class.
    /// </summary>
    public UserRegistrationException()
        : base("L'inscription de l'utilisateur a échoué.", "USER_REGISTRATION_FAILED", 400)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRegistrationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UserRegistrationException(string message)
        : base(message, "USER_REGISTRATION_FAILED", 400)
    {
    }
}

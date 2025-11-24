namespace SGE.Core.Exceptions;

/// <summary>
///     Exception thrown when a user is not found in the system.
/// </summary>
public class UserNotFoundException : SgeException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserNotFoundException" /> class.
    /// </summary>
    public UserNotFoundException()
        : base("Utilisateur introuvable.", "USER_NOT_FOUND", 404)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserNotFoundException" /> class with a specified user identifier.
    /// </summary>
    /// <param name="userId">The identifier of the user that was not found.</param>
    public UserNotFoundException(string userId)
        : base($"Utilisateur avec l'ID '{userId}' introuvable.", "USER_NOT_FOUND", 404)
    {
    }
}

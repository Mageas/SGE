namespace SGE.Core.Exceptions;

public class ValidationException : SgeException
{
    public ValidationException(Dictionary<string, List<string>> errors)
        : base("Une ou plusieurs erreurs de validation sont survenues.", "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : this(new Dictionary<string, List<string>> { { propertyName, new List<string> { errorMessage } } })
    {
    }

    public Dictionary<string, List<string>> Errors { get; }
}
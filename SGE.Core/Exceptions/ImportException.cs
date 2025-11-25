namespace SGE.Core.Exceptions;

public class ImportException(Dictionary<string, List<string>> errors)
    : SgeException("Des erreurs sont survenues lors de l'importation.", "IMPORT_ERROR", 400)
{
    public Dictionary<string, List<string>> Errors { get; } = errors;
}

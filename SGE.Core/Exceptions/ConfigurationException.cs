namespace SGE.Core.Exceptions;

public class ConfigurationException : SgeException
{
    public ConfigurationException(string message)
        : base(message, "CONFIGURATION_ERROR", 500)
    {
    }
}

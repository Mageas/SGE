namespace SGE.Core.Exceptions;

public class BusinessRuleException : SgeException
{
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message, errorCode, 400)
    {
    }
}
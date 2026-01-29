namespace OnlineBookManagementSystem.Core.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }

        protected DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }



    public class InvalidBusinessRuleException : DomainException
    {
        public InvalidBusinessRuleException(string rule, string reason)
            : base($"Business rule violation: {rule}. Reason: {reason}")
        {
        }
    }
}
using Stacy.Core.Exceptions;

namespace Stacy.Core.Data
{
    public abstract class Validator
    {
        public List<string> Messages { get; set; } = new List<string>();
        public string ErrorMessage => "There has been a validation error";

        public CoreUserAggregateException GetAggregateException()
        {
            if (!Messages.Any())
                return null;

            var exceptions = Messages.Select(m => new CoreUserException(m));

            return new CoreUserAggregateException(ErrorMessage, exceptions);
        }

        public List<CoreUserException> GetExceptionList()
        {
            return Messages.Select(m => new CoreUserException(m)).ToList();
        }
    }
}
